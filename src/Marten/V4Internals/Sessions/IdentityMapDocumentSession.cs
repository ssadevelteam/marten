using Marten.Services;
using Marten.Storage;

namespace Marten.V4Internals.Sessions
{
    public class IdentityMapDocumentSession: NewDocumentSession
    {
        public IdentityMapDocumentSession(IDocumentStore store, IManagedConnection database, ISerializer serializer, ITenant tenant, StoreOptions options) : base(store, database, serializer, tenant, options)
        {
        }

        protected override IDocumentStorage<T> selectStorage<T>(DocumentProvider<T> provider)
        {
            return provider.IdentityMap;
        }
    }
}
