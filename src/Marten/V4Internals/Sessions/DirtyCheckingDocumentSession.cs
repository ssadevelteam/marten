using System;
using Marten.Services;
using Marten.Storage;

namespace Marten.V4Internals.Sessions
{
    public class DirtyCheckingDocumentSession: NewDocumentSession
    {
        public DirtyCheckingDocumentSession(DocumentStore store, SessionOptions sessionOptions, IManagedConnection database, ITenant tenant) : base(store, sessionOptions, database, tenant)
        {
        }

        protected override IDocumentStorage<T> selectStorage<T>(DocumentProvider<T> provider)
        {
            return provider.DirtyTracking;
        }

        protected override void clearDirtyChecking()
        {
            // TODO -- do something here!
        }

        protected override void ejectById<T>(long id)
        {
            throw new NotImplementedException();
        }

        protected override void ejectById<T>(int id)
        {
            throw new NotImplementedException();
        }

        protected override void ejectById<T>(Guid id)
        {
            throw new NotImplementedException();
        }

        protected override void ejectById<T>(string id)
        {
            throw new NotImplementedException();
        }
    }
}
