using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Marten.Linq;
using Marten.Schema.Arguments;
using Marten.Services;
using Marten.Storage;
using Marten.Util;
using Marten.V4Internals.Linq;
using Npgsql;
using Remotion.Linq.Clauses;

namespace Marten.V4Internals.Sessions
{
    public abstract class MartenSessionBase: IMartenSession
    {
        private readonly IProviderGraph _providers;
        private bool _disposed;
        public VersionTracker Versions { get; } = new VersionTracker();
        public IManagedConnection Database { get; }
        public ISerializer Serializer { get; }
        public Dictionary<Type, object> ItemMap { get; } = new Dictionary<Type, object>();
        public ITenant Tenant { get; }
        public StoreOptions Options { get; }

        protected MartenSessionBase(IManagedConnection database, ISerializer serializer, ITenant tenant,
            StoreOptions options)
        {
            _providers = tenant.Providers ?? throw new ArgumentNullException(nameof(ITenant.Providers));

            Database = database;
            Serializer = serializer;
            Tenant = tenant;
            Options = options;
        }

        protected abstract IDocumentStorage<T> selectStorage<T>(DocumentProvider<T> provider);


        public IDocumentStorage StorageFor(Type documentType)
        {
            // TODO -- possible optimization opportunity
            return typeof(StorageFinder<>).CloseAndBuildAs<IStorageFinder>(documentType).Find(this);
        }

        private interface IStorageFinder
        {
            IDocumentStorage Find(MartenSessionBase session);
        }

        private class StorageFinder<T>: IStorageFinder
        {
            public IDocumentStorage Find(MartenSessionBase session)
            {
                return session.storageFor<T>();
            }
        }

        protected IDocumentStorage<T, TId> storageFor<T, TId>()
        {
            var storage = storageFor<T>();
            if (storage is IDocumentStorage<T, TId> s) return s;

            throw new InvalidOperationException($"The identity type for {typeof(T).FullName} is {storage.IdType.FullName}, but {typeof(TId).FullName} was used as the Id type");
        }

        protected IDocumentStorage<T> storageFor<T>()
        {
            return selectStorage(_providers.StorageFor<T>());
        }




        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Database?.Dispose();
            GC.SuppressFinalize(this);
        }

        protected void assertNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("This session has been disposed");
        }



    }
}
