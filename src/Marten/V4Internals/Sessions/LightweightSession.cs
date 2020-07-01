using System;
using Marten.Services;
using Marten.Storage;

namespace Marten.V4Internals.Sessions
{
    public class LightweightSession: NewDocumentSession
    {
        public LightweightSession(IDocumentStore store, IManagedConnection database, ISerializer serializer, ITenant tenant, StoreOptions options) : base(store, database, serializer, tenant, options)
        {
        }

        protected override IDocumentStorage<T> selectStorage<T>(DocumentProvider<T> provider)
        {
            return provider.Lightweight;
        }

        public override void Eject<T>(T document)
        {
            // Nothing
        }

        public override void EjectAllOfType(Type type)
        {
            // Nothing
        }
    }
}
