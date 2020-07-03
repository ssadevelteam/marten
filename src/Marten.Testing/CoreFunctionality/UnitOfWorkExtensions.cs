using System;
using System.Linq;
using Marten.Services;
using Marten.Testing.Documents;
using Marten.V4Internals;
using Shouldly;

namespace Marten.Testing.CoreFunctionality
{
    public static class UnitOfWorkExtensions
    {
        public static void ShouldHaveUpsertFor<T>(this IDocumentSession session, T document)
        {
            session.PendingChanges.Operations()
                .OfType<IDocumentStorageOperation>()

                .ShouldContain(x => x.Role() == StorageRole.Upsert && document.Equals(x.Document));
        }

        public static void ShouldHaveInsertFor<T>(this IDocumentSession session, T document)
        {
            session.PendingChanges.Operations()
                .OfType<IDocumentStorageOperation>()

                .ShouldContain(x => x.Role() == StorageRole.Insert && document.Equals(x.Document));
        }

        public static void ShouldHaveUpdateFor<T>(this IDocumentSession session, T document)
        {
            session.PendingChanges.Operations()
                .OfType<IDocumentStorageOperation>()

                .ShouldContain(x => x.Role() == StorageRole.Update && document.Equals(x.Document));
        }

        public static void ShouldHaveDeleteFor(this IDocumentSession session, User user)
        {
            session.PendingChanges.Operations()
                .OfType<DeleteOne<User, Guid>>()
                .ShouldContain(x => x.Id == user.Id);
        }

        public static void ShouldHaveDeleteFor(this IDocumentSession session, Target target)
        {
            session.PendingChanges.Operations()
                .OfType<DeleteOne<Target, Guid>>()
                .ShouldContain(x => x.Id == target.Id);
        }
    }
}
