using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
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

        protected override void processChangeTrackers()
        {
            foreach (var tracker in ChangeTrackers)
            {
                if (tracker.DetectChanges(this, out var operation))
                {
                    _unitOfWork.Add(operation);
                }
            }
        }

        protected override void resetDirtyChecking(UnitOfWork unitOfWork)
        {
            foreach (var tracker in ChangeTrackers)
            {
                tracker.Reset(this);
            }

            var knownDocuments = ChangeTrackers.Select(x => x.Document).ToArray();

            var operations =unitOfWork.AllOperations
                .OfType<IDocumentStorageOperation>()
                .Where(x => !knownDocuments.Contains(x.Document));

            foreach (var operation in operations)
            {
                var tracker = operation.ToTracker(this);
                ChangeTrackers.Add(tracker);
            }
        }


        private void removeTrackerFor<T>(T document)
        {
            ChangeTrackers.RemoveAll(x => ReferenceEquals(x.Document, document));
        }

        protected override void ejectById<T>(long id)
        {
            if (ItemMap.TryGetValue(typeof(T), out var dict))
            {
                if (dict is Dictionary<long, T> d)
                {
                    if (d.ContainsKey(id))
                    {
                        removeTrackerFor(d[id]);
                        d.Remove(id);
                    }

                }
            }
        }

        protected override void ejectById<T>(int id)
        {
            if (ItemMap.TryGetValue(typeof(T), out var dict))
            {
                if (dict is Dictionary<int, T> intd)
                {
                    if (intd.ContainsKey(id))
                    {
                        removeTrackerFor(intd[id]);
                        intd.Remove(id);
                    }

                }
                else if (dict is Dictionary<long, T> longd)
                {
                    var lid = (long)id;
                    if (longd.ContainsKey(lid))
                    {
                        removeTrackerFor(longd[lid]);
                        longd.Remove(lid);
                    }
                }
            }


        }

        protected override void ejectById<T>(Guid id)
        {
            if (ItemMap.TryGetValue(typeof(T), out var dict))
            {
                if (dict is Dictionary<Guid, T> d)
                {
                    if (d.ContainsKey(id))
                    {
                        removeTrackerFor(d[id]);
                        d.Remove(id);
                    }
                }
            }
        }

        protected override void ejectById<T>(string id)
        {
            if (ItemMap.TryGetValue(typeof(T), out var dict))
            {
                if (dict is Dictionary<string, T> d)
                {
                    if (d.ContainsKey(id))
                    {
                        removeTrackerFor(d[id]);
                        d.Remove(id);
                    }
                }
            }
        }
    }
}
