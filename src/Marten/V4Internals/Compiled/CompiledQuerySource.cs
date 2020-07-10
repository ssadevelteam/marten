using System;

namespace Marten.V4Internals.Compiled
{
    public abstract class CompiledQuerySource<TOut, TQuery> : ICompiledQuerySource
    {
        public Type QueryType => typeof(TQuery);

        public abstract IQueryHandler<TOut> BuildHandler(TQuery query, IMartenSession session);

        public IQueryHandler Build(object query, IMartenSession session)
        {
            return BuildHandler((TQuery)query, session);
        }
    }
}