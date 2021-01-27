using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Marten.Events.Projections;
using Marten.Internal.Sessions;
using Microsoft.Extensions.Logging;

namespace Marten.Events.Daemon
{

    internal interface IProjectionAgent
    {
        Task Stop();

        Task TryRestart();

        Task<long> Start(ShardStateTracker tracker);
        ShardName ShardName { get; }
    }

    // TODO -- need a separate Drain() v. Stop() method
    // TODO -- need a Dispose that really cleans things off. May need/want IAsyncDisposable
    internal class ProjectionAgent : IProjectionUpdater, IObserver<ShardState>, IProjectionAgent
    {
        private readonly DocumentStore _store;
        private readonly IAsyncProjectionShard _projectionShard;
        private readonly ILogger<IProjection> _logger;
        private readonly CancellationToken _cancellation;
        private ITargetBlock<EventRange> _hopper;
        private readonly ProjectionController _controller;
        private readonly ActionBlock<Command> _commandBlock;
        private readonly TransformBlock<EventRange, EventRange> _loader;
        private EventFetcher _fetcher;
        private ShardStateTracker _tracker;
        private IDisposable _subscription;

        // ReSharper disable once ContextualLoggerProblem
        public ProjectionAgent(DocumentStore store, IAsyncProjectionShard projectionShard, ILogger<IProjection> logger, CancellationToken cancellation)
        {
            _store = store;
            _projectionShard = projectionShard;
            _logger = logger;
            _cancellation = cancellation;

            var singleFile = new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1,
                CancellationToken = _cancellation
            };

            _commandBlock = new ActionBlock<Command>(processCommand, singleFile);

            _controller =
                new ProjectionController(projectionShard.Name, this, projectionShard.Options);

            _loader = new TransformBlock<EventRange, EventRange>(loadEvents, singleFile);
        }

        // TODO -- use IAsyncDisposable
        public async Task Stop()
        {
            _logger.LogInformation($"Shutting down projection shard {_projectionShard.Name}");


            _commandBlock.Complete();
            await _commandBlock.Completion;
            _logger.LogDebug("Shut down the command block");

            _loader.Complete();
            await _loader.Completion;

            _logger.LogDebug("Shut down the loader block");

            _hopper.Complete();
            await _hopper.Completion;

            _logger.LogDebug("Shut down the hopper block");

            await _projectionShard.Stop();

            _logger.LogDebug("Shut down the shard itself");

            _subscription.Dispose();

        }

        private async Task<EventRange> loadEvents(EventRange range)
        {
            if (_cancellation.IsCancellationRequested) return null;

            try
            {
                // TODO -- resiliency here.
                await _fetcher.Load(range, _cancellation);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error loading events for " + range);
                // TODO -- retry? circuit breaker?
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Loaded events for {range}");
            }

            return range;
        }

        private void processCommand(Command command) => command.Apply(_controller);

        public AgentStatus Status { get; private set; }


        public void StartRange(EventRange range)
        {
            if (_cancellation.IsCancellationRequested) return;

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Enqueued processing of " + range);
            }

            _loader.Post(range);
        }

        public async Task<long> Start(ShardStateTracker tracker)
        {
            _logger.LogInformation($"Starting projection agent for '{_projectionShard.Name}'");

            _tracker = tracker;


            _fetcher = new EventFetcher(_store, _projectionShard.EventFilters);
            _hopper = _projectionShard.Start(this, _logger, _cancellation);
            _loader.LinkTo(_hopper);

            var lastCommitted = await _store.Events.ProjectionProgressFor(_projectionShard.Name);

            _commandBlock.Post(Command.Started(tracker.HighWaterMark, lastCommitted));

            _subscription = _tracker.Subscribe(this);

            _logger.LogInformation($"Projection agent for '{_projectionShard.Name}' has started from sequence {lastCommitted} and a high water mark of {tracker.HighWaterMark}");

            Status = AgentStatus.Running;

            Position = lastCommitted;
            return lastCommitted;
        }

        public Task TryRestart()
        {
            throw new NotImplementedException();
        }

        void IObserver<ShardState>.OnCompleted()
        {
            // Nothing
        }

        void IObserver<ShardState>.OnError(Exception error)
        {
            // Nothing
        }

        void IObserver<ShardState>.OnNext(ShardState value)
        {
            if (value.ShardName == ShardState.HighWaterMark)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"Projection Shard '{ShardName}' received high water mark at {value.Sequence}");
                }

                _commandBlock.Post(
                    Command.HighWaterMarkUpdated(value.Sequence));
            }
        }

        public ShardName ShardName => _projectionShard.Name;

        public ProjectionUpdateBatch StartNewBatch(EventRange range)
        {
            var session = _store.LightweightSession();
            return new ProjectionUpdateBatch(_store.Events, (DocumentSessionBase) session, range);
        }

        public async Task ExecuteBatch(ProjectionUpdateBatch batch)
        {
            if (_cancellation.IsCancellationRequested) return;

            await batch.Queue.Completion;

            using (var session = (DocumentSessionBase)_store.LightweightSession())
            {
                try
                {
                    await session.ExecuteBatchAsync(batch, _cancellation);

                    _logger.LogInformation($"Shard '{ShardName}': Executed updates for {batch.Range}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failure in shard '{ShardName}' trying to execute an update batch for {batch.Range}");
                    // TODO -- error handling

                    throw;
                }
            }

            batch.Dispose();


            Position = batch.Range.SequenceCeiling;


            _tracker.Publish(new ShardState(ShardName, batch.Range.SequenceCeiling));

            _commandBlock.Post(Command.Completed(batch.Range));
        }

        public long Position { get; set; }
    }
}
