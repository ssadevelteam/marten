using Marten.V4Internals.Linq;

namespace Marten.Linq.QueryHandlers
{
    internal class LinqConstants
    {
        internal static readonly string StatsColumn = "count(1) OVER() as total_rows";
        internal static readonly string IdListTableName = "mt_temp_id_list";

        internal static readonly V4Internals.ISelector<string> StringValueSelector =
            new ScalarStringSelectClause(string.Empty, string.Empty);
    }
}
