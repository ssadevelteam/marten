using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Marten.Linq;
using Marten.Services.Includes;
using Marten.Util;

namespace Marten
{
    public static class CompiledQueryExtensions
    {
        public static string AsJson<T>(this T target)
        {
            throw new NotImplementedException();
        }

        public static IMartenQueryable<T> Include<T, TQuery>(this IQueryable<T> queryable, Expression<Func<T, object>> idSource, Func<TQuery, object> callback,
            JoinType joinType = JoinType.Inner)
        {
            throw new NotImplementedException();
        }

        public static IMartenQueryable<T> Stats<T, TQuery>(this IQueryable<T> queryable, Expression<Func<TQuery, QueryStatistics>> stats)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<string> AsJson<T>(this IMartenQueryable<T> queryable)
        {
            return queryable.Select(x => x.AsJson());
        }

        public static IQueryable<string> AsJson<T>(this IQueryable<T> queryable)
        {
            return queryable.Select(x => x.AsJson());
        }

        public static IQueryable<string> AsJson<T>(this IOrderedQueryable<T> queryable)
        {
            return queryable.Select(x => x.AsJson());
        }

        public static string ToJsonArray<T>(this IQueryable<T> queryable)
        {
            return queryable.As<IMartenQueryable<T>>().ToJsonArray();
        }

        public static string ToJsonArray<T>(this IOrderedQueryable<T> queryable)
        {
            return queryable.As<IMartenQueryable<T>>().ToJsonArray();
        }

        public static Task<string> ToJsonArrayAsync<T>(this IQueryable<T> queryable, CancellationToken token = default)
        {
            return queryable.As<IMartenQueryable<T>>().ToJsonArrayAsync(token);
        }
    }
}
