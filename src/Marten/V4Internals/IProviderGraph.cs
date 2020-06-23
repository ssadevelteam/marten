namespace Marten.V4Internals
{
    public interface IProviderGraph
    {
        DocumentProvider<T> StorageFor<T>();
    }
}
