using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;
using Marten.Linq;
using Marten.Services;
using Marten.Transforms;
using Marten.Util;
using Marten.V4Internals.Linq.Includes;
using Npgsql;
using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;

namespace Marten.V4Internals.Linq
{
    public class V4Queryable<T>: QueryableBase<T>, IMartenQueryable<T>
    {
        private readonly V4QueryProvider _provider;
        private readonly IMartenSession _session;

        public V4Queryable(IMartenSession session, V4QueryProvider provider, Expression expression): base(provider,
            expression)
        {
            _session = session;
            _provider = provider;
        }

        public V4Queryable(IMartenSession session): base(new V4QueryProvider(session))
        {
            _session = session;
            _provider = Provider.As<V4QueryProvider>();
        }

        public V4Queryable(IMartenSession session, Expression expression): base(new V4QueryProvider(session),
            expression)
        {
            _session = session;
            _provider = Provider.As<V4QueryProvider>();
        }

        public QueryStatistics Statistics
        {
            get => _provider.Statistics;
            set => _provider.Statistics = value;
        }

        public Task<IReadOnlyList<TResult>> ToListAsync<TResult>(CancellationToken token)
        {
            return _provider.ExecuteAsync<IReadOnlyList<TResult>>(Expression, token);
        }

        public Task<bool> AnyAsync(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<bool>(Expression, token, new AnyResultOperator());
        }

        public Task<int> CountAsync(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<int>(Expression, token, new CountResultOperator());
        }

        public Task<long> CountLongAsync(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<long>(Expression, token, new LongCountResultOperator());
        }

        public Task<TResult> FirstAsync<TResult>(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<TResult>(Expression, token, new FirstResultOperator(false));
        }

        public Task<TResult> FirstOrDefaultAsync<TResult>(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<TResult>(Expression, token, new FirstResultOperator(true));
        }

        public Task<TResult> SingleAsync<TResult>(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<TResult>(Expression, token, new SingleResultOperator(false));
        }

        public Task<TResult> SingleOrDefaultAsync<TResult>(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<TResult>(Expression, token, new SingleResultOperator(true));
        }

        public Task<TResult> SumAsync<TResult>(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<TResult>(Expression, token, new SumResultOperator());
        }

        public Task<TResult> MinAsync<TResult>(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<TResult>(Expression, token, new MinResultOperator());
        }

        public Task<TResult> MaxAsync<TResult>(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<TResult>(Expression, token, new MaxResultOperator());
        }

        public Task<double> AverageAsync(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<double>(Expression, token, new AverageResultOperator());
        }

        public string ToJsonArray()
        {
            // TODO -- flyweight for the operator
            return _provider.Execute<string>(Expression, new AsJsonResultOperator(null));
        }

        public Task<string> ToJsonArrayAsync(CancellationToken token)
        {
            // TODO -- flyweight for the operator
            return _provider.ExecuteAsync<string>(Expression, token, new AsJsonResultOperator(null));
        }

        public QueryPlan Explain(FetchType fetchType = FetchType.FetchMany,
            Action<IConfigureExplainExpressions> configureExplain = null)
        {
            var command = ToPreviewCommand(fetchType);

            return _session.Database.ExplainQuery(command, configureExplain);
        }

        public IQueryable<TDoc> TransformTo<TDoc>(string transformName)
        {
            return this.Select(x => x.TransformTo<T, TDoc>(transformName));
        }

        public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, Action<TInclude> callback)
        {
            var storage = (IDocumentStorage<TInclude>)_session.StorageFor(typeof(TInclude));
            var identityField = _session.StorageFor(typeof(T)).Fields.FieldFor(idSource);

            var include = new Include<TInclude>(_provider.Includes.Count, storage, identityField, callback);
            _provider.Includes.Add(include);

            return this;
        }

        public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, IList<TInclude> list)
        {
            return Include<TInclude>(idSource, list.Add);
        }

        public IMartenQueryable<T> Include<TInclude, TKey>(Expression<Func<T, object>> idSource,
            IDictionary<TKey, TInclude> dictionary)
        {
            var storage = (IDocumentStorage<TInclude>)_session.StorageFor(typeof(TInclude));

            if (storage is IDocumentStorage<TInclude, TKey> s)
            {
                var identityField = _session.StorageFor(typeof(T)).Fields.FieldFor(idSource);

                void Callback(TInclude item)
                {
                    var id = s.Identity(item);
                    dictionary[id] = item;
                }

                var include = new Include<TInclude>(_provider.Includes.Count, storage, identityField, Callback);
                _provider.Includes.Add(include);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Id/Document type mismatch. The id type for the included document type {typeof(TInclude).FullNameInCode()} is {storage.IdType.FullNameInCode()}");
            }


            return this;
        }

        public IMartenQueryable<T> Stats(out QueryStatistics stats)
        {
            Statistics = new QueryStatistics();
            stats = Statistics;

            return this;
        }

        public NpgsqlCommand ToPreviewCommand(FetchType fetchType)
        {
            var builder = new LinqHandlerBuilder(_session, Expression);
            var command = new NpgsqlCommand();
            var sql = new CommandBuilder(command);
            builder.BuildDiagnosticCommand(fetchType, sql);
            command.CommandText = sql.ToString();
            return command;
        }
    }
}
