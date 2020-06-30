using System;
using Marten.Linq;
using Marten.Util;

namespace Marten.V4Internals.Linq
{
    public class DataSelectClause<T> : ISelectClause
    {
        public DataSelectClause(string from)
        {
            FromObject = from;
        }

        public DataSelectClause(string from, string selectionText)
        {
            FromObject = from;
            SelectionText = selectionText;
        }

        public Type SelectedType => typeof(T);

        public string SelectionText { get; protected set; } = "select d.data from ";

        public string FromObject { get; }
        public void WriteSelectClause(CommandBuilder sql)
        {
            sql.Append(SelectionText);
            sql.Append(FromObject);
            sql.Append(" as d");
        }

        public string[] SelectFields()
        {
            return new string[] {"data"};
        }

        public ISelector BuildSelector(IMartenSession session)
        {
            return new SerializationSelector<T>(session.Serializer);
        }

        public IQueryHandler<TResult> BuildHandler<TResult>(IMartenSession session, Statement statement,
            Statement currentStatement)
        {
            var selector = new SerializationSelector<T>(session.Serializer);

            return LinqHandlerBuilder.BuildHandler<T, TResult>(selector, statement);
        }

        public ISelectClause UseStatistics(QueryStatistics statistics)
        {
            return new StatsSelectClause<T>(this, statistics);
        }
    }
}
