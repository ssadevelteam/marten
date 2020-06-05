using System.Collections.Generic;
using System.Linq;

namespace Marten
{
    public static class DocumentSessionExtensions
    {
        /// <summary>
        /// Explicitly marks a document as needing to be inserted or updated upon the next call to SaveChanges()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static void Store<T>(this IDocumentSession session, IEnumerable<T> entities)
        {
            session.Store(entities.ToArray());
        }

        /// <summary>
        /// Explicitly marks a document as needing to be inserted or updated upon the next call to SaveChanges()
        /// to a specific tenant
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static void Store<T>(this IDocumentSession session, string tenantId, IEnumerable<T> entities)
        {
            session.Store(tenantId, entities.ToArray());
        }

        /// <summary>
        /// Explicitly marks a document as needing to be inserted upon the next call to SaveChanges().
        /// Will throw an exception if the document already exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static void Insert<T>(this IDocumentSession session, IEnumerable<T> entities)
        {
            session.Insert(entities.ToArray());
        }

        /// <summary>
        /// Explicitly marks a document as needing to be updated upon the next call to SaveChanges().
        /// Will throw an exception if the document does not already exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static void Update<T>(this IDocumentSession session, IEnumerable<T> entities)
        {
            session.Update(entities.ToArray());
        }
    }
}
