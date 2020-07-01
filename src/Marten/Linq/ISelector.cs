using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Marten.Schema;
using Marten.Services;
using Marten.Util;
using Npgsql;

namespace Marten.Linq
{
    public interface ISelector
    {
        string[] SelectFields();

        void WriteSelectClause(CommandBuilder sql, IQueryableDocument mapping);
    }

    public interface ISelector<T>: ISelector
    {
        T Resolve(DbDataReader reader, IIdentityMap map, QueryStatistics stats);

        Task<T> ResolveAsync(DbDataReader reader, IIdentityMap map, QueryStatistics stats, CancellationToken token);
    }


    public static class SelectorExtensions
    {
        // Polyfill for the Async Daemon where it doesn't do any harm
        public static string ToSelectClause(this ISelector selector, IQueryableDocument mapping)
        {
            var builder = new CommandBuilder(new NpgsqlCommand());
            selector.WriteSelectClause(builder, mapping);

            return builder.ToString();
        }

        public static IReadOnlyList<T> Read<T>(this ISelector<T> selector, DbDataReader reader, IIdentityMap map, QueryStatistics stats)
        {
            var list = new List<T>();

            while (reader.Read())
            {
                list.Add(selector.Resolve(reader, map, stats));
            }

            return list;
        }

        public static async Task<IReadOnlyList<T>> ReadAsync<T>(this ISelector<T> selector, DbDataReader reader, IIdentityMap map, QueryStatistics stats, CancellationToken token)
        {
            var list = new List<T>();

            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                list.Add(await selector.ResolveAsync(reader, map, stats, token).ConfigureAwait(false));
            }

            return list;
        }
    }
}
