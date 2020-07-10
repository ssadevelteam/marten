using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Marten.Linq;
using Marten.V4Internals.Linq.Includes;
using Npgsql;

namespace Marten.V4Internals.Compiled
{
    public class CompiledQueryPlan : ICompiledQueryPlan
    {
        public Type QueryType { get; }
        public Type OutputType { get; }

        public CompiledQueryPlan(Type queryType, Type outputType)
        {
            QueryType = queryType;
            OutputType = outputType;
        }

        public IList<ParameterMap> Parameters { get; } = new List<ParameterMap>();


        public NpgsqlCommand Command { get; set; }

        public string CorrectedCommandText()
        {
            var text = Command.CommandText;
            for (int i = 0; i < Command.Parameters.Count; i++)
            {
                text = text.Replace(":p" + i, "?");
            }

            return text;
        }

        IQueryHandler ICompiledQueryPlan.Prototype => HandlerPrototype;

        public IQueryHandler HandlerPrototype { get; set; }

        public MemberInfo StatisticsMember { get; set; }

        public IList<MemberInfo> IncludeMembers { get; } = new List<MemberInfo>();

        public IList<IIncludePlan> IncludePlans { get; } = new List<IIncludePlan>();
        public MemberInfo[] SpecialMembers()
        {
            return specialMembers().ToArray();
        }

        private IEnumerable<MemberInfo> specialMembers()
        {
            if (StatisticsMember != null) yield return StatisticsMember;

            foreach (var includeMember in IncludeMembers)
            {
                yield return includeMember;
            }
        }

        public QueryStatistics GetStatisticsIfAny(object query)
        {
            if (StatisticsMember is PropertyInfo p) return (QueryStatistics)p.GetValue(query) ?? new QueryStatistics();

            if (StatisticsMember is FieldInfo f) return (QueryStatistics)f.GetValue(query) ?? new QueryStatistics();

            return null;
        }
    }
}
