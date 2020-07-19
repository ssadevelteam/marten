using System;
using System.Collections.Generic;
using Marten.Services;
using Marten.Storage;

namespace Marten.V4Internals.Sessions
{
    public class IdentityMapDocumentSession: NewDocumentSession
    {
        public IdentityMapDocumentSession(DocumentStore store, SessionOptions sessionOptions, IManagedConnection database, ITenant tenant) : base(store, sessionOptions, database, tenant)
        {
        }

        protected override IDocumentStorage<T> selectStorage<T>(DocumentProvider<T> provider)
        {
            return provider.IdentityMap;
        }

        protected override void ejectById<T>(long id)
        {
            StorageFor<T>().EjectById(this, id);
        }

        protected override void ejectById<T>(int id)
        {
            StorageFor<T>().EjectById(this, id);
        }

        protected override void ejectById<T>(Guid id)
        {
            StorageFor<T>().EjectById(this, id);
        }

        protected override void ejectById<T>(string id)
        {
            StorageFor<T>().EjectById(this, id);
        }
    }
}
