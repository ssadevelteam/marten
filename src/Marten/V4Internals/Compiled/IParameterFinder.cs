using System;
using System.Collections.Generic;
using Marten.Linq;
using Npgsql;

namespace Marten.V4Internals.Compiled
{
    public interface IParameterFinder
    {
        SearchMatch TryMatch(object query);

        IEnumerable<ParameterMap> SinglePassMatch(object query, NpgsqlCommand command);

        IEnumerable<ParameterMap> MultiplePassMatch<TDoc, TOut>(
            Func<ICompiledQuery<TDoc, TOut>, NpgsqlCommand> source, Type queryType);

        Type DotNetType { get; }
    }
}
