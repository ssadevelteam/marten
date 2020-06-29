using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Marten.Events;
using Marten.Linq;
using Marten.Patching;
using Marten.Services;
using Marten.Storage;
using Marten.Util;
using Npgsql;

namespace Marten.V4Internals.Sessions
{
    public abstract class NewDocumentSession: QuerySession, IDocumentSession, IUnitOfWork
    {
        private readonly IList<IStorageOperation> _pendingOperations = new List<IStorageOperation>();


        protected NewDocumentSession(IDocumentStore store, IManagedConnection database, ISerializer serializer, ITenant tenant,
            StoreOptions options) : base(store, database, serializer, tenant, options)
        {
        }


        public void Delete<T>(T entity)
        {
            assertNotDisposed();
            var deletion = storageFor<T>().DeleteForDocument(entity);
            _pendingOperations.Add(deletion);
        }

        public void Delete<T>(int id)
        {
            assertNotDisposed();
            var deletion = storageFor<T, int>().DeleteForId(id);
            _pendingOperations.Add(deletion);
        }

        public void Delete<T>(long id)
        {
            assertNotDisposed();
            var deletion = storageFor<T, long>().DeleteForId(id);
            _pendingOperations.Add(deletion);
        }

        public void Delete<T>(Guid id)
        {
            assertNotDisposed();
            var deletion = storageFor<T, Guid>().DeleteForId(id);
            _pendingOperations.Add(deletion);
        }

        public void Delete<T>(string id)
        {
            assertNotDisposed();
            var deletion = storageFor<T, string>().DeleteForId(id);
            _pendingOperations.Add(deletion);
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
            assertNotDisposed();

            var storage = storageFor<T>();
            foreach (var entity in entities)
            {
                storage.Store(this, entity);
                var op = storage.Upsert(entity, this);
                _pendingOperations.Add(op);
            }
        }

        public void Store<T>(params T[] entities)
        {
            assertNotDisposed();

            var storage = storageFor<T>();
            foreach (var entity in entities)
            {
                storage.Store(this, entity);
                var op = storage.Upsert(entity, this);
                _pendingOperations.Add(op);
            }
        }

        public void Store<T>(string tenantId, IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        public void Store<T>(string tenantId, params T[] entities)
        {
            throw new NotImplementedException();
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
            assertNotDisposed();

            var storage = storageFor<T>();
            foreach (var entity in entities)
            {
                storage.Store(this, entity);
                var op = storage.Insert(entity, this);
                _pendingOperations.Add(op);
            }
        }

        public void Insert<T>(params T[] entities)
        {
            assertNotDisposed();

            var storage = storageFor<T>();
            foreach (var entity in entities)
            {
                storage.Store(this, entity);
                var op = storage.Insert(entity, this);
                _pendingOperations.Add(op);
            }
        }

        public void Update<T>(IEnumerable<T> entities)
        {
            assertNotDisposed();

            var storage = storageFor<T>();
            foreach (var entity in entities)
            {
                storage.Store(this, entity);
                var op = storage.Update(entity, this);
                _pendingOperations.Add(op);
            }
        }

        public void Update<T>(params T[] entities)
        {
            assertNotDisposed();

            var storage = storageFor<T>();
            foreach (var entity in entities)
            {
                storage.Store(this, entity);
                var op = storage.Update(entity, this);
                _pendingOperations.Add(op);
            }
        }

        public void InsertObjects(IEnumerable<object> documents)
        {
            throw new NotImplementedException();
        }

        public IUnitOfWork PendingChanges { get; }
        public void StoreObjects(IEnumerable<object> documents)
        {
            throw new NotImplementedException();
        }

        public IEventStore Events { get; }
        public ConcurrencyChecks Concurrency { get; }
        public IList<IDocumentSessionListener> Listeners { get; }
        public IPatchExpression<T> Patch<T>(int id)
        {
            throw new NotImplementedException();
        }

        public IPatchExpression<T> Patch<T>(long id)
        {
            throw new NotImplementedException();
        }

        public IPatchExpression<T> Patch<T>(string id)
        {
            throw new NotImplementedException();
        }

        public IPatchExpression<T> Patch<T>(Guid id)
        {
            throw new NotImplementedException();
        }

        public IPatchExpression<T> Patch<T>(Expression<Func<T, bool>> @where)
        {
            throw new NotImplementedException();
        }

        public IPatchExpression<T> Patch<T>(IWhereFragment fragment)
        {
            throw new NotImplementedException();
        }

        public void QueueOperation(Services.IStorageOperation storageOperation)
        {
            throw new NotImplementedException();
        }

        public void Eject<T>(T document)
        {
            throw new NotImplementedException();
        }

        public void EjectAllOfType(Type type)
        {
            throw new NotImplementedException();
        }

        IEnumerable<IDeletion> IUnitOfWork.Deletions()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IDeletion> IUnitOfWork.DeletionsFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IDeletion> IUnitOfWork.DeletionsFor(Type documentType)
        {
            throw new NotImplementedException();
        }

        IEnumerable<object> IUnitOfWork.Updates()
        {
            throw new NotImplementedException();
        }

        IEnumerable<object> IUnitOfWork.Inserts()
        {
            throw new NotImplementedException();
        }

        IEnumerable<T> IUnitOfWork.UpdatesFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<T> IUnitOfWork.InsertsFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<T> IUnitOfWork.AllChangedFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<EventStream> IUnitOfWork.Streams()
        {
            throw new NotImplementedException();
        }

        IEnumerable<PatchOperation> IUnitOfWork.Patches()
        {
            throw new NotImplementedException();
        }

        IEnumerable<Services.IStorageOperation> IUnitOfWork.Operations()
        {
            throw new NotImplementedException();
        }

        IEnumerable<Services.IStorageOperation> IUnitOfWork.OperationsFor<T>()
        {
            throw new NotImplementedException();
        }

        IEnumerable<Services.IStorageOperation> IUnitOfWork.OperationsFor(Type documentType)
        {
            throw new NotImplementedException();
        }
    }
}
