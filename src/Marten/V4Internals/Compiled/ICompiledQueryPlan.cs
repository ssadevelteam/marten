using System.Collections.Generic;
using System.Reflection;
using Marten.V4Internals.Linq.Includes;
using Npgsql;

namespace Marten.V4Internals.Compiled
{
    public interface ICompiledQueryPlan
    {
        IList<ParameterMap> Parameters { get; }
        NpgsqlCommand Command { get; set; }
        MemberInfo StatisticsMember { get; set; }
        IList<MemberInfo> IncludeMembers { get; }
        string CorrectedCommandText();

        IQueryHandler Prototype { get; }
        IList<IIncludePlan> IncludePlans { get; }

        MemberInfo[] SpecialMembers();
    }
}
