using System.Collections.Generic;
using Marten.Events.Daemon;
using Marten.Storage;

namespace Marten.Events.Projections
{
    public abstract class ProjectionSource
    {
        public string ProjectionName { get; protected set; }

        protected ProjectionSource(string projectionName)
        {
            ProjectionName = projectionName;
        }

        internal abstract IProjection Build(DocumentStore store);
        internal abstract IReadOnlyList<IAsyncProjectionShard> AsyncProjectionShards(IDocumentStore store, ITenancy tenancy);

        public AsyncOptions Options { get; } = new AsyncOptions();

        internal virtual void AssertValidity()
        {
            // Nothing
        }

        internal virtual IEnumerable<string> ValidateConfiguration(StoreOptions options)
        {
            // Nothing
            yield break;
        }
    }
}
