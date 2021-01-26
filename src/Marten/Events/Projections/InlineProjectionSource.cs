using System.Collections.Generic;
using LamarCodeGeneration;
using Marten.Events.Daemon;
using Marten.Linq.SqlGeneration;
using Marten.Storage;

namespace Marten.Events.Projections
{
    internal class InlineProjectionSource: ProjectionSource
    {
        private readonly IProjection _projection;

        public InlineProjectionSource(IProjection projection) : base(projection.GetType().FullNameInCode())
        {
            _projection = projection;
        }

        internal override IProjection Build(DocumentStore store)
        {
            return _projection;
        }

        internal override IReadOnlyList<IAsyncProjectionShard> AsyncProjectionShards(IDocumentStore store, ITenancy tenancy)
        {
            var shard = new AsyncProjectionShard(ProjectionName, _projection, System.Array.Empty<ISqlFragment>(), (DocumentStore) store, Options);
            return new List<IAsyncProjectionShard> {shard};
        }
    }
}
