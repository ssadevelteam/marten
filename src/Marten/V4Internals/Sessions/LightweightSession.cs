using System;
using Marten.Services;
using Marten.Storage;

namespace Marten.V4Internals.Sessions
{
    public class LightweightSession: NewDocumentSession
    {
        public LightweightSession(DocumentStore store, SessionOptions sessionOptions, IManagedConnection database, ITenant tenant) : base(store, sessionOptions, database, tenant)
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

        protected override void ejectById<T>(long id)
        {
            // Nothing
        }

        protected override void ejectById<T>(int id)
        {
            // Nothing
        }

        protected override void ejectById<T>(Guid id)
        {
            // Nothing
        }

        protected override void ejectById<T>(string id)
        {
            // Nothing
        }
    }
}
