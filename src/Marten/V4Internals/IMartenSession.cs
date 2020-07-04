using System;
using System.Collections.Generic;
using Marten.Services;
using Marten.Storage;
using Marten.V4Internals.DirtyTracking;

namespace Marten.V4Internals
{
    public interface IMartenSession: IDisposable
    {
        ISerializer Serializer { get; }
        Dictionary<Type, object> ItemMap { get; }
        ITenant Tenant { get; }

        VersionTracker Versions { get; }

        IManagedConnection Database { get; }

        StoreOptions Options { get; }

        IList<IChangeTracker> ChangeTrackers { get; }
        IDocumentStorage StorageFor(Type documentType);


        void MarkAsAddedForStorage(object id, object document);

        void MarkAsDocumentLoaded(object id, object document);
        IDocumentStorage<T> StorageFor<T>();
    }
}
