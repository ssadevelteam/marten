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
            if (ItemMap.TryGetValue(typeof(T), out var dict))
            {
                if (dict is Dictionary<long, T> longd)
                {
                    longd.Remove(id);
                }
                else if (dict is Dictionary<int, T> intd)
                {
                    intd.Remove((int) id);
                }
            }
        }

        protected override void ejectById<T>(int id)
        {
            if (ItemMap.TryGetValue(typeof(T), out var dict))
            {
                if (dict is Dictionary<int, T> d) d.Remove(id);
            }
        }

        protected override void ejectById<T>(Guid id)
        {
            if (ItemMap.TryGetValue(typeof(T), out var dict))
            {
                if (dict is Dictionary<Guid, T> d) d.Remove(id);
            }
        }

        protected override void ejectById<T>(string id)
        {
            if (ItemMap.TryGetValue(typeof(T), out var dict))
            {
                if (dict is Dictionary<string, T> d) d.Remove(id);
            }
        }
    }
}
