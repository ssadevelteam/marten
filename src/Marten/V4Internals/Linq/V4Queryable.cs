using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LamarCodeGeneration.Util;
using Marten.Linq;
using Marten.Linq.Model;
using Marten.Services;
using Marten.Services.Includes;
using Marten.Util;
using Marten.V4Internals.Sessions;
using Npgsql;
using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;

namespace Marten.V4Internals.Linq
{
    public class V4Queryable<T> : QueryableBase<T>, IMartenQueryable<T>
    {
        private readonly IMartenSession _session;
        private readonly V4QueryProvider _provider;

        public V4Queryable(IMartenSession session, V4QueryProvider provider, Expression expression) : base(provider, expression)
        {
            _session = session;
            _provider = provider;
        }

        public V4Queryable(IMartenSession session) : base(new V4QueryProvider(session))
        {
            _session = session;
            _provider = Provider.As<V4QueryProvider>();
        }

        public V4Queryable(IMartenSession session, Expression expression) : base(new V4QueryProvider(session), expression)
        {
            _session = session;
            _provider = Provider.As<V4QueryProvider>();
        }



        public IEnumerable<IIncludeJoin> Includes { get; }

        public QueryStatistics Statistics
        {
            get
            {
                return _provider.Statistics;
            }
            set
            {
                _provider.Statistics = value;
            }
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

        public QueryPlan Explain(FetchType fetchType = FetchType.FetchMany, Action<IConfigureExplainExpressions> configureExplain = null)
        {
            var command = ToPreviewCommand(fetchType);

            return _session.Database.ExplainQuery(command, configureExplain);
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

        public IQueryable<TDoc> TransformTo<TDoc>(string transformName)
        {
            throw new NotImplementedException();
        }

        public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, Action<TInclude> callback, JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, IList<TInclude> list, JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IMartenQueryable<T> Include<TInclude, TKey>(Expression<Func<T, object>> idSource, IDictionary<TKey, TInclude> dictionary,
            JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public IMartenQueryable<T> Stats(out QueryStatistics stats)
        {
            Statistics = new QueryStatistics();
            stats = Statistics;

            return this;
        }

        // TODO -- try to get rid of this
        public LinqQuery<T> ToLinqQuery()
        {
            throw new NotImplementedException();
        }
    }
}
