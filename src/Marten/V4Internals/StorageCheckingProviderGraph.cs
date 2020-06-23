using System;
using Baseline;
using Marten.Util;

namespace Marten.V4Internals
{
    public class StorageCheckingProviderGraph: IProviderGraph
    {
        private ImHashMap<Type, object> _storage = ImHashMap<Type, object>.Empty;
        private readonly ITenantStorage _tenant;
        private readonly IProviderGraph _inner;

        public StorageCheckingProviderGraph(ITenantStorage tenant, IProviderGraph inner)
        {
            _tenant = tenant;
            _inner = inner;
        }

        public DocumentProvider<T> StorageFor<T>()
        {
            if (_storage.TryFind(typeof(T), out var stored))
            {
                return stored.As<DocumentProvider<T>>();
            }

            _tenant.EnsureStorageExists(typeof(T));
            var persistence = _inner.StorageFor<T>();

            _storage = _storage.AddOrUpdate(typeof(T), persistence);

            return persistence;
        }
    }
}
