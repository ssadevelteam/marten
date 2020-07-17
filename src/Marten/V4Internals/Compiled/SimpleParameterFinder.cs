using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Marten.Linq;
using Npgsql;

namespace Marten.V4Internals.Compiled
{
    public class SimpleParameterFinder<T> : IParameterFinder
    {
        private readonly Func<int, T[]> _uniqueValues;

        public SimpleParameterFinder(Func<int, T[]> uniqueValues)
        {
            _uniqueValues = uniqueValues;
        }

        public Type DotNetType => typeof(T);

        public Queue<object> UniqueValueQueue()
        {
            return new Queue<object>(_uniqueValues(100).OfType<object>());
        }

        public bool AreValuesUnique(object query, CompiledQueryPlan plan)
        {
            var members = findMembers(plan);

            if (!members.Any()) return true;

            return members.Select(x => x.GetValue(query))
                .Distinct().Count() == members.Length;
        }

        private static IQueryMember<T>[] findMembers(CompiledQueryPlan plan)
        {
            return plan.Parameters.OfType<IQueryMember<T>>().ToArray();
        }

    }
}
