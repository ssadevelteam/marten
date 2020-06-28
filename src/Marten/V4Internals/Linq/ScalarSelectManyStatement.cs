using Marten.Util;

namespace Marten.V4Internals.Linq
{
    public class ScalarSelectManyStatement<T>: Statement
    {
        public static string ToLocator(ISerializer serializer)
        {
            if (typeof(T) == typeof(string)) return "data";

            return $"CAST(data as {TypeMappings.GetPgType(typeof(T), serializer.EnumStorage)})";
        }

        public ScalarSelectManyStatement(Statement parent, ISerializer serializer) : base(new ScalarSelectClause<T>(ToLocator(serializer), parent.ExportName), null)
        {
        }
    }
}
