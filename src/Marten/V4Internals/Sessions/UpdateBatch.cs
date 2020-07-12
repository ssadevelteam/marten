using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten.Schema.Arguments;
using Marten.Services;
using Marten.Util;
using Npgsql;

namespace Marten.V4Internals.Sessions
{
    public class UpdateBatch
    {
        private readonly IReadOnlyList<IStorageOperation> _operations;

        private readonly IList<Exception> _exceptions = new List<Exception>();

        public UpdateBatch(IReadOnlyList<IStorageOperation> operations)
        {
            _operations = operations;
        }

        public void ApplyChanges(IMartenSession session)
        {




            if (_operations.Count < session.Options.UpdateBatchSize)
            {
                var command = buildCommand(session, _operations);
                using var reader = session.Database.ExecuteReader(command);
                applyCallbacks(_operations, reader);
            }
            else
            {
                var count = 0;

                while (count < _operations.Count)
                {
                    var operations = _operations
                        .Skip(count)
                        .Take(session.Options.UpdateBatchSize)
                        .ToArray();

                    var command = buildCommand(session, operations);
                    using var reader = session.Database.ExecuteReader(command);
                    applyCallbacks(operations, reader);

                    count += session.Options.UpdateBatchSize;
                }
            }

            if (_exceptions.Any()) throw new AggregateException(_exceptions);
        }

        public async Task ApplyChangesAsync(IMartenSession session, CancellationToken token)
        {
            if (_operations.Count < session.Options.UpdateBatchSize)
            {
                var command = buildCommand(session, _operations);
                using var reader = await session.Database.ExecuteReaderAsync(command, token).ConfigureAwait(false);
                await applyCallbacksAsync(_operations, reader, token).ConfigureAwait(false);
            }
            else
            {
                var count = 0;

                while (count < _operations.Count)
                {
                    var operations = _operations
                        .Skip(count)
                        .Take(session.Options.UpdateBatchSize)
                        .ToArray();

                    var command = buildCommand(session, operations);
                    using var reader = await session.Database.ExecuteReaderAsync(command, token).ConfigureAwait(false);
                    await applyCallbacksAsync(operations, reader, token).ConfigureAwait(false);

                    count += session.Options.UpdateBatchSize;
                }
            }

            if (_exceptions.Any()) throw new AggregateException(_exceptions);
        }

        private void applyCallbacks(IEnumerable<IStorageOperation> operations, DbDataReader reader)
        {
            var first = operations.First();

            if (!(first is NoDataReturnedCall))
            {
                first.Postprocess(reader, _exceptions);
                reader.NextResult();
            }

            foreach (var operation in operations.Skip(1))
            {
                if (!(operation is NoDataReturnedCall))
                {
                    operation.Postprocess(reader, _exceptions);
                    reader.NextResult();
                }
            }
        }

        private async Task applyCallbacksAsync(IEnumerable<IStorageOperation> operations, DbDataReader reader, CancellationToken token)
        {
            var first = operations.First();

            if (!(first is NoDataReturnedCall))
            {
                await first.PostprocessAsync(reader, _exceptions, token).ConfigureAwait(false);
                await reader.NextResultAsync(token).ConfigureAwait(false);
            }

            foreach (var operation in operations.Skip(1))
            {
                if (!(operation is NoDataReturnedCall))
                {
                    await operation.PostprocessAsync(reader, _exceptions, token).ConfigureAwait(false);
                    await reader.NextResultAsync(token).ConfigureAwait(false);
                }
            }
        }

        private NpgsqlCommand buildCommand(IMartenSession session, IEnumerable<IStorageOperation> operations)
        {
            var command = new NpgsqlCommand();
            var builder = new CommandBuilder(command);

            foreach (var operation in operations)
            {
                operation.ConfigureCommand(builder, session);
                builder.Append(';');
            }

            // Duplication here!
            command.CommandText = builder.ToString();

            // TODO -- Like this to be temporary
            if (command.CommandText.Contains(CommandBuilder.TenantIdArg))
            {
                command.AddNamedParameter(TenantIdArgument.ArgName, session.Tenant.TenantId);
            }

            return command;
        }


    }
}
