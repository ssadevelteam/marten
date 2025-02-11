using Marten.Linq.Fields;
using Marten.Linq.SqlGeneration;
using Weasel.Postgresql;
using Marten.Util;
using Weasel.Postgresql.SqlGeneration;

namespace Marten.Linq.Filters
{
    // TODO -- remove the usage of IField
    public class IsNotNullFilter: IReversibleWhereFragment
    {
        public IsNotNullFilter(IField field)
        {
            Field = field;
        }

        public IField Field { get; }

        public void Apply(CommandBuilder builder)
        {
            builder.Append(Field.RawLocator);
            builder.Append(" is not null");
        }

        public bool Contains(string sqlText)
        {
            return Field.Contains(sqlText);
        }

        public ISqlFragment Reverse()
        {
            return new IsNullFilter(Field);
        }
    }
}
