using System;
using System.Collections.Generic;
using Marten.Linq;
using Npgsql;

namespace Marten.V4Internals.Compiled
{
    public interface IParameterFinder
    {
        Type DotNetType { get; }
        bool AreValuesUnique(object query, CompiledQueryPlan plan);
        Queue<object> UniqueValueQueue();
    }
}
