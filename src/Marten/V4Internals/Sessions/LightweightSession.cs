namespace Marten.V4Internals.Sessions
{
    public class LightweightSession: NewDocumentSession
    {
        public LightweightSession(IDocumentStore store, IDatabase database, ISerializer serializer, ITenant tenant, IProviderGraph provider, StoreOptions options) : base(store, database, serializer, tenant, provider, options)
        {
        }

        protected override IDocumentStorage<T> selectStorage<T>(DocumentProvider<T> provider)
        {
            return provider.Lightweight;
        }
    }
}
