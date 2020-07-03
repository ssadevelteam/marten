using Marten.V4Internals.DirtyTracking;
using Marten.V4Internals.Sessions;

namespace Marten.V4Internals
{
    public interface IDocumentStorageOperation : IStorageOperation
    {
        object Document { get; }
        IChangeTracker ToTracker(IMartenSession session);
    }
}
