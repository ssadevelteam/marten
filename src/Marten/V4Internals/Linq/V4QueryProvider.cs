using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Marten.Linq;
using Marten.Schema.Arguments;
using Marten.Storage;
using Marten.Util;
using Marten.V4Internals.Linq.Includes;
using Npgsql;
using Remotion.Linq.Clauses;

namespace Marten.V4Internals.Linq
{
    public class V4QueryProvider: IQueryProvider
    {
        private readonly IMartenSession _session;

        public V4QueryProvider(IMartenSession session)
        {
            _session = session;
        }

        internal QueryStatistics Statistics { get; set; }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new V4Queryable<TElement>(_session, this, expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var builder = new LinqHandlerBuilder(_session, expression);
            var handler = builder.BuildHandler<TResult>(Statistics, Includes);

            return ExecuteHandler(handler);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            var builder = new LinqHandlerBuilder(_session, expression);
            var handler = builder.BuildHandler<TResult>(Statistics, Includes);

            return ExecuteHandlerAsync(handler, token);
        }

        public TResult Execute<TResult>(Expression expression, ResultOperatorBase op)
        {
            var builder = new LinqHandlerBuilder(_session, expression, op);
            var handler = builder.BuildHandler<TResult>(Statistics, Includes);

            return ExecuteHandler(handler);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token, ResultOperatorBase op)
        {
            var builder = new LinqHandlerBuilder(_session, expression, op);
            var handler = builder.BuildHandler<TResult>(Statistics, Includes);

            // TODO -- worry about QueryStatistics later
            return ExecuteHandlerAsync(handler, token);
        }

        public async Task<T> ExecuteHandlerAsync<T>(IQueryHandler<T> handler, CancellationToken token)
        {
            var cmd = new NpgsqlCommand();
            var builder = new CommandBuilder(cmd);
            handler.ConfigureCommand(builder, _session);

            cmd.CommandText = builder.ToString();

            // TODO -- Like this to be temporary
            if (cmd.CommandText.Contains(CommandBuilder.TenantIdArg))
            {
                cmd.AddNamedParameter(TenantIdArgument.ArgName, _session.Tenant.TenantId);
            }

            using (var reader = await _session.Database.ExecuteReaderAsync(cmd, token).ConfigureAwait(false))
            {
                return await handler.HandleAsync(reader, _session, token).ConfigureAwait(false);
            }
        }

        public T ExecuteHandler<T>(IQueryHandler<T> handler)
        {
            var cmd = new NpgsqlCommand();
            var builder = new CommandBuilder(cmd);
            handler.ConfigureCommand(builder, _session);

            cmd.CommandText = builder.ToString();

            // TODO -- Like this to be temporary
            if (cmd.CommandText.Contains(CommandBuilder.TenantIdArg))
            {
                cmd.AddNamedParameter(TenantIdArgument.ArgName, _session.Tenant.TenantId);
            }

            using (var reader = _session.Database.ExecuteReader(cmd))
            {
                return handler.Handle(reader, _session);
            }
        }

        public IList<IInclude> Includes { get; } = new List<IInclude>();

    }
}
