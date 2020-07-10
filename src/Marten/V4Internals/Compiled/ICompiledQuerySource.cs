using System;

namespace Marten.V4Internals.Compiled
{
    public interface ICompiledQuerySource
    {
        Type QueryType { get; }
        IQueryHandler Build(object query, IMartenSession session);
    }
}
