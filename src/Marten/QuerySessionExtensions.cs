using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Marten
{
    public static class QuerySessionExtensions
    {
        private static readonly MethodInfo QueryMethod = typeof(IQuerySession).GetMethod(nameof(IQuerySession.Query), new[] { typeof(string), typeof(object[]) });
        private static readonly MethodInfo QueryMethodAsync = typeof(IQuerySession).GetMethod(nameof(IQuerySession.QueryAsync), new[] { typeof(string), typeof(CancellationToken), typeof(object[]) });

        public static IReadOnlyList<object> Query(this IQuerySession session, Type type, string sql, params object[] parameters)
        {
            return (IReadOnlyList<object>)QueryMethod.MakeGenericMethod(type).Invoke(session, new object[] { sql, parameters });
        }

        public static async Task<IReadOnlyList<object>> QueryAsync(this IQuerySession session, Type type, string sql, CancellationToken token = default(CancellationToken), params object[] parameters)
        {
            var task = (Task)QueryMethodAsync.MakeGenericMethod(type).Invoke(session, new object[] { sql, token, parameters });
            await task.ConfigureAwait(false);
            return (IReadOnlyList<object>)task.GetType().GetProperty("Result").GetValue(task);
        }

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReadOnlyList<T> LoadMany<T>(this IQuerySession session, IEnumerable<string> ids)
        {
            return session.LoadMany<T>(ids.ToArray());
        }



        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReadOnlyList<T> LoadMany<T>(this IQuerySession session, IEnumerable<Guid> ids)
        {
            return session.LoadMany<T>(ids.ToArray());
        }


        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReadOnlyList<T> LoadMany<T>(this IQuerySession session, IEnumerable<int> ids)
        {
            return session.LoadMany<T>(ids.ToArray());
        }

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReadOnlyList<T> LoadMany<T>(this IQuerySession session, IEnumerable<long> ids)
        {
            return session.LoadMany<T>(ids.ToArray());
        }


        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<IReadOnlyList<T>> LoadManyAsync<T>(this IQuerySession session, IEnumerable<string> ids)
        {
            return session.LoadManyAsync<T>(ids.ToArray());
        }

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<IReadOnlyList<T>> LoadManyAsync<T>(this IQuerySession session, IEnumerable<Guid> ids)
        {
            return session.LoadManyAsync<T>(ids.ToArray());
        }


        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<IReadOnlyList<T>> LoadManyAsync<T>(this IQuerySession session, IEnumerable<int> ids)
        {
            return session.LoadManyAsync<T>(ids.ToArray());
        }

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<IReadOnlyList<T>> LoadManyAsync<T>(this IQuerySession session, IEnumerable<long> ids)
        {
            return session.LoadManyAsync<T>(ids.ToArray());
        }

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<IReadOnlyList<T>> LoadManyAsync<T>(IQuerySession session, CancellationToken token, IEnumerable<string> ids)
        {
            return session.LoadManyAsync<T>(token, ids.ToArray());
        }

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<IReadOnlyList<T>> LoadManyAsync<T>(IQuerySession session, CancellationToken token, IEnumerable<Guid> ids)
        {

            return session.LoadManyAsync<T>(token, ids.ToArray());
        }

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<IReadOnlyList<T>> LoadManyAsync<T>(IQuerySession session, CancellationToken token, IEnumerable<int> ids)
        {
            return session.LoadManyAsync<T>(token, ids.ToArray());
        }

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<IReadOnlyList<T>> LoadManyAsync<T>(IQuerySession session, CancellationToken token, IEnumerable<long> ids)
        {
            return session.LoadManyAsync<T>(token, ids.ToArray());
        }
    }
}
