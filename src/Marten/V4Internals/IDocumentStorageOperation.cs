namespace Marten.V4Internals
{
    public interface IDocumentStorageOperation : IStorageOperation
    {
        object Document { get; }
    }
}
