using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Baseline;
using Marten.Internal.Sessions;
using Marten.Linq;
using Marten.Linq.Filters;
using Marten.Linq.SqlGeneration;
using Marten.Schema;
using Marten.Schema.Identity;
using Marten.Storage;
using Marten.Util;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Weasel.Postgresql.SqlGeneration;

namespace Marten.Transforms
{
    internal class DocumentTransforms: IDocumentTransforms
    {
        private readonly DocumentStore _store;
        private readonly ITenant _tenant;

        public DocumentTransforms(DocumentStore store, ITenant tenant)
        {
            _store = store;
            _tenant = tenant;
        }

        public void All<T>(string transformName)
        {
            var transform = _tenant.TransformFor(transformName);
            var storage = _tenant.StorageFor<T>();

            using var session = (DocumentSessionBase)_store.LightweightSession();
            var operation = new DocumentTransformOperationFragment(storage, transform);
            var statement = new StatementOperation(storage, operation);

            // To bake in the default document filtering here
            statement.CompileLocal(session);
            session.WorkTracker.Add(statement);
            session.SaveChanges();
        }

        public void Tenant<T>(string transformName, string tenantId)
        {
            Where<T>(transformName, x => x.TenantIsOneOf(tenantId));
        }

        public void Tenants<T>(string transformName, params string[] tenantIds)
        {
            Where<T>(transformName, x => x.TenantIsOneOf(tenantIds));
        }

        public void Where<T>(string transformName, Expression<Func<T, bool>> @where)
        {
            var transform = _tenant.TransformFor(transformName);


            using var session = (DocumentSessionBase)_store.LightweightSession();

            var storage = session.StorageFor<T>();
            var operation = new DocumentTransformOperationFragment(storage, transform);

            var statement = new StatementOperation(storage, operation);
            statement.ApplyFiltering(session, @where);
            session.WorkTracker.Add(statement);
            session.SaveChanges();
        }


        public void Document<T>(string transformName, string id)
        {
            transformOne<T>(transformName, new ByStringFilter(id));
        }

        private void transformOne<T>(string transformName, ISqlFragment filter)
        {
            var transform = _tenant.TransformFor(transformName);

            using var session = (DocumentSessionBase)_store.LightweightSession();
            var storage = session.StorageFor<T>();
            var operation = new DocumentTransformOperationFragment(storage, transform);

            var statement = new StatementOperation(storage, operation) {Where = filter};

            // To bake in the default document filtering here
            statement.CompileLocal(session);
            session.WorkTracker.Add(statement);
            session.SaveChanges();
        }

        public void Document<T>(string transformName, int id)
        {
            transformOne<T>(transformName, new ByIntFilter(id));
        }

        public void Document<T>(string transformName, long id)
        {
            transformOne<T>(transformName, new ByLongFilter(id));
        }

        public void Document<T>(string transformName, Guid id)
        {
            transformOne<T>(transformName, new ByGuidFilter(id));
        }
    }
}
