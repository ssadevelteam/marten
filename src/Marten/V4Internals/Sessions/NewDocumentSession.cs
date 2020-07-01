using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Marten.Events;
using Marten.Linq;
using Marten.Patching;
using Marten.Services;
using Marten.Storage;
using Marten.Util;
using Marten.V4Internals.Linq;
using Npgsql;
using Remotion.Linq.Clauses;

namespace Marten.V4Internals.Sessions
{
    public abstract class NewDocumentSession: QuerySession, IDocumentSession
    {
        private readonly IList<IStorageOperation> _pendingOperations = new List<IStorageOperation>();


        protected NewDocumentSession(IDocumentStore store, IManagedConnection database, ISerializer serializer, ITenant tenant,
            StoreOptions options) : base(store, database, serializer, tenant, options)
        {
            // TODO -- need to take in concurrency checks
            // Take in SessionOptions here
        }


        public void Delete<T>(T entity)
        {
            assertNotDisposed();
            var deletion = storageFor<T>().DeleteForDocument(entity);
            _pendingOperations.Add(deletion);

            // TODO -- eject from identity map
        }

        public void Delete<T>(int id)
        {
            assertNotDisposed();
            var deletion = storageFor<T, int>().DeleteForId(id);
            _pendingOperations.Add(deletion);

            // TODO -- eject from identity map
        }

        public void Delete<T>(long id)
        {
            assertNotDisposed();
            var deletion = storageFor<T, long>().DeleteForId(id);
            _pendingOperations.Add(deletion);

            // TODO -- eject from identity map
        }

        public void Delete<T>(Guid id)
        {
            assertNotDisposed();
            var deletion = storageFor<T, Guid>().DeleteForId(id);
            _pendingOperations.Add(deletion);

            // TODO -- eject from identity map
        }

        public void Delete<T>(string id)
        {
            assertNotDisposed();
            var deletion = storageFor<T, string>().DeleteForId(id);
            _pendingOperations.Add(deletion);

            // TODO -- eject from identity map
        }

        public void DeleteWhere<T>(Expression<Func<T, bool>> expression)
        {
            assertNotDisposed();

            // TODO -- memoize the parser
            var parser = new MartenExpressionParser(Options.Serializer(), Options);

            // TODO -- this could be cleaner maybe?
            var documentStorage = storageFor<T>();
            var @where = parser.ParseWhereFragment(documentStorage.Fields, expression);
            var deletion = documentStorage.DeleteForWhere(@where);
            _pendingOperations.Add(deletion);
        }

        public void SaveChanges()
        {
            // TODO -- move all this to new UnitOfWork!

            var command = new NpgsqlCommand();
            var builder = new CommandBuilder(command);
            foreach (var operation in _pendingOperations)
            {
                operation.ConfigureCommand(builder, this);
                builder.Append(";");
            }

            var exceptions = new List<Exception>();

            // TODO -- hokey!
            command.CommandText = builder.ToString();
            using (var reader = Database.ExecuteReader(command))
            {
                _pendingOperations[0].Postprocess(reader, exceptions);
                for (int i = 1; i < _pendingOperations.Count; i++)
                {
                    reader.NextResult();
                    _pendingOperations[i].Postprocess(reader, exceptions);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        public async Task SaveChangesAsync(CancellationToken token = default)
        {
            var command = new NpgsqlCommand();
            var builder = new CommandBuilder(command);
            foreach (var operation in _pendingOperations)
            {
                operation.ConfigureCommand(builder, this);
                builder.Append(";");
            }

            var exceptions = new List<Exception>();

            // TODO -- hokey!
            command.CommandText = builder.ToString();
            using (var reader = await Database.ExecuteReaderAsync(command, token).ConfigureAwait(false))
            {
                await _pendingOperations[0].PostprocessAsync(reader, exceptions, token).ConfigureAwait(false);
                for (var i = 1; i < _pendingOperations.Count; i++)
                {
                    await reader.NextResultAsync(token).ConfigureAwait(false);
                    await _pendingOperations[i].PostprocessAsync(reader, exceptions, token).ConfigureAwait(false);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        public void Store<T>(IEnumerable<T> entities)
        {
            Store(entities?.ToArray());
        }

        public void Store<T>(params T[] entities)
        {
            assertNotDisposed();

            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if (typeof(T).IsGenericEnumerable())
                throw new ArgumentOutOfRangeException(typeof(T).Name, "Do not use IEnumerable<T> here as the document type. Either cast entities to an array instead or use the IEnumerable<T> Store() overload instead.");

            store(entities);
        }


        private void store<T>(IEnumerable<T> entities)
        {
            assertNotDisposed();

            if (typeof(T) == typeof(object))
            {
                StoreObjects(entities.OfType<object>());
            }
            else
            {
                var storage = storageFor<T>();

                foreach (var entity in entities)
                {
                    var upsert = storage.Upsert(entity, this);

                    // Put it in the identity map -- if necessary
                    storage.Store(this, entity);

                    _pendingOperations.Add(upsert);
                }
            }
        }


        public void Store<T>(string tenantId, IEnumerable<T> entities)
        {
            Store(tenantId, entities?.ToArray());
        }

        public void Store<T>(string tenantId, params T[] entities)
        {
            assertNotDisposed();

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (typeof(T).IsGenericEnumerable())
            {
                throw new ArgumentOutOfRangeException(typeof(T).Name, "Do not use IEnumerable<T> here as the document type. Cast entities to an array or use the IEnumerable<T> Store() overload instead.");
            }

            var tenant = DocumentStore.Tenancy[tenantId];

            var storage = tenant.StorageFor<T>();

            foreach (var entity in entities)
            {
                var op = storage.Upsert(entity, this, tenant);
                storage.Store(this, entity);
                _pendingOperations.Add(op);
            }
        }

        public void Store<T>(T entity, Guid version)
        {
            assertNotDisposed();

            var storage = storageFor<T>();
            storage.Store(this, entity, version);
            var op = storage.Upsert(entity, this);
            _pendingOperations.Add(op);
        }

        public void Insert<T>(IEnumerable<T> entities)
        {
            Insert(entities.ToArray());


        }

        public void Insert<T>(params T[] entities)
        {
            assertNotDisposed();

            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if (typeof(T).IsGenericEnumerable())
            {
                throw new ArgumentOutOfRangeException(typeof(T).Name, "Do not use IEnumerable<T> here as the document type. You may need to cast entities to an array instead.");
            }

            if (typeof(T) == typeof(object))
            {
                InsertObjects(entities.OfType<object>());
            }
            else
            {
                var storage = storageFor<T>();

                foreach (var entity in entities)
                {
                    storage.Store(this, entity);
                    var op = storage.Insert(entity, this);
                    _pendingOperations.Add(op);
                }
            }
        }

        public void Update<T>(IEnumerable<T> entities)
        {
            Update(entities.ToArray());
        }

        public void Update<T>(params T[] entities)
        {
            assertNotDisposed();

            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if (typeof(T).IsGenericEnumerable())
            {
                throw new ArgumentOutOfRangeException(typeof(T).Name, "Do not use IEnumerable<T> here as the document type. You may need to cast entities to an array instead.");
            }

            if (typeof(T) == typeof(object))
            {
                InsertObjects(entities.OfType<object>());
            }
            else
            {
                var storage = storageFor<T>();

                foreach (var entity in entities)
                {
                    storage.Store(this, entity);
                    var op = storage.Update(entity, this);
                    _pendingOperations.Add(op);
                }
            }
        }

        public void InsertObjects(IEnumerable<object> documents)
        {
            assertNotDisposed();

            documents.Where(x => x != null).GroupBy(x => x.GetType()).Each(group =>
            {
                var handler = typeof(InsertHandler<>).CloseAndBuildAs<IHandler>(group.Key);
                handler.Store(this, group);
            });
        }

        public IUnitOfWork PendingChanges => throw new NotImplementedException();

        public void StoreObjects(IEnumerable<object> documents)
        {
            assertNotDisposed();

            documents.Where(x => x != null).GroupBy(x => x.GetType()).Each(group =>
            {
                var handler = typeof(Handler<>).CloseAndBuildAs<IHandler>(group.Key);
                handler.Store(this, group);
            });
        }

        internal interface IHandler
        {
            void Store(IDocumentSession session, IEnumerable<object> objects);
        }

        internal class Handler<T>: IHandler
        {
            public void Store(IDocumentSession session, IEnumerable<object> objects)
            {
                session.Store(objects.OfType<T>().ToArray());
            }
        }

        internal class InsertHandler<T>: IHandler
        {
            public void Store(IDocumentSession session, IEnumerable<object> objects)
            {
                session.Insert(objects.OfType<T>().ToArray());
            }
        }

        public IEventStore Events { get; }
        public ConcurrencyChecks Concurrency { get; set; } = ConcurrencyChecks.Enabled;
        public IList<IDocumentSessionListener> Listeners { get; }
        public IPatchExpression<T> Patch<T>(int id)
        {
            return patchById<T>(id);
        }

        public IPatchExpression<T> Patch<T>(long id)
        {
            return patchById<T>(id);
        }

        public IPatchExpression<T> Patch<T>(string id)
        {
            return patchById<T>(id);
        }

        public IPatchExpression<T> Patch<T>(Guid id)
        {
            return patchById<T>(id);
        }

        public IPatchExpression<T> Patch<T>(Expression<Func<T, bool>> filter)
        {
            assertNotDisposed();

            var queryable = Query<T>().Where(filter);
            var model = MartenQueryParser.Flyweight.GetParsedQuery(queryable.Expression);

            var storage = storageFor<T>();

            // TODO -- parser needs to be a singleton in the system
            var @where = storage.BuildWhereFragment(model, new MartenExpressionParser(Serializer, Options));

            return new PatchExpression<T>(@where, this);
        }

        public IPatchExpression<T> Patch<T>(IWhereFragment fragment)
        {
            assertNotDisposed();

            return new PatchExpression<T>(fragment, this);
        }

        private IPatchExpression<T> patchById<T>(object id)
        {
            assertNotDisposed();

            var @where = new WhereFragment("d.id = ?", id);
            return new PatchExpression<T>(@where, this);
        }


        public void QueueOperation(IStorageOperation storageOperation)
        {
            _pendingOperations.Add(storageOperation);
        }

        public virtual void Eject<T>(T document)
        {
            storageFor<T>().Eject(this, document);
        }

        public virtual void EjectAllOfType(Type type)
        {
            ItemMap.Remove(type);
        }

        // TODO -- this needs to be called at the end of a
        public void EjectPatchedTypes(IUnitOfWork changes)
        {
            var patchedTypes = changes.Patches().Select(x => x.DocumentType).Distinct().ToArray();
            foreach (var type in patchedTypes)
            {
                EjectAllOfType(type);
            }
        }


    }
}
