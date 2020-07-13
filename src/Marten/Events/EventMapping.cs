using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using LamarCodeGeneration;
using Marten.Linq;
using Marten.Linq.Fields;
using Marten.Schema;
using Marten.Schema.Identity;
using Marten.Services;
using Marten.Storage;
using Marten.Util;
using Marten.V4Internals;
using Marten.V4Internals.Linq;
using Npgsql;
using NpgsqlTypes;
using Remotion.Linq;

namespace Marten.Events
{
    public abstract class EventMapping: IDocumentMapping, IQueryableDocument
    {
        protected readonly EventGraph _parent;
        protected readonly DocumentMapping _inner;

        protected EventMapping(EventGraph parent, Type eventType)
        {
            _parent = parent;
            DocumentType = eventType;

            EventTypeName = eventType.IsGenericType ? eventType.ShortNameInCode() : DocumentType.Name.ToTableAlias();
            IdMember = DocumentType.GetProperty(nameof(IEvent.Id));

            _inner = new DocumentMapping(eventType, parent.Options);
        }

        public IDocumentMapping Root => this;
        public Type DocumentType { get; }
        public string EventTypeName { get; set; }
        public string Alias => EventTypeName;
        public MemberInfo IdMember { get; }
        public NpgsqlDbType IdType { get; } = NpgsqlDbType.Uuid;
        public TenancyStyle TenancyStyle { get; } = TenancyStyle.Single;

        Type IDocumentMapping.IdType => typeof(Guid);

        public DbObjectName Table => new DbObjectName(_parent.DatabaseSchemaName, "mt_events");
        public DuplicatedField[] DuplicatedFields { get; }
        public DeleteStyle DeleteStyle { get; }

        public PropertySearching PropertySearching { get; } = PropertySearching.JSON_Locator_Only;

        public string[] SelectFields()
        {
            return new[] { "id", "data" };
        }

        public IField FieldFor(Expression expression)
        {
            return FieldFor(FindMembers.Determine(expression));
        }

        public IField FieldFor(IEnumerable<MemberInfo> members)
        {
            return _inner.FieldFor(members);
        }

        public IField FieldFor(MemberInfo member)
        {
            return _inner.FieldFor(member);
        }

        public IField FieldFor(string memberName)
        {
            throw new NotSupportedException();
        }

        public IWhereFragment FilterDocuments(QueryModel model, IWhereFragment query)
        {
            return new CompoundWhereFragment("and", DefaultWhereFragment(), query);
        }

        public IWhereFragment DefaultWhereFragment()
        {
            return new WhereFragment($"d.type = '{EventTypeName}'");
        }

        public void DeleteAllDocuments(ITenant factory)
        {
            factory.RunSql($"delete from mt_events where type = '{Alias}'");
        }

        public IdAssignment<T> ToIdAssignment<T>(ITenant tenant)
        {
            throw new NotSupportedException();
        }

        public IQueryableDocument ToQueryableDocument()
        {
            return this;
        }

    }

    public class EventMapping<T>: EventMapping, IDocumentStorage<T> where T : class
    {
        private readonly string _tableName;
        private Type _idType;

        public EventMapping(EventGraph parent) : base(parent, typeof(T))
        {
            var schemaName = parent.DatabaseSchemaName;
            _tableName = schemaName == StoreOptions.DefaultDatabaseSchemaName ? "mt_events" : $"{schemaName}.mt_events";

            _idType = parent.StreamIdentity == StreamIdentity.AsGuid ? typeof(Guid) : typeof(string);
        }

        [Obsolete("Lot of methods removed in v4 we can remove")]
        public Type TopLevelBaseType => DocumentType;

        public NpgsqlCommand LoaderCommand(object id)
        {
            return new NpgsqlCommand($"select d.data, d.id from {_tableName} as d where id = :id and type = '{Alias}'").With("id", id);
        }

        public NpgsqlCommand LoadByArrayCommand<TKey>(TKey[] ids)
        {
            return new NpgsqlCommand($"select d.data, d.id from {_tableName} as d where id = ANY(:ids) and type = '{Alias}'").With("ids", ids);
        }

        public object Identity(object document)
        {
            return document.As<IEvent>().Id;
        }

        public IStorageOperation DeletionForId(object id)
        {
            throw new NotSupportedException("You cannot delete events at this time");
        }

        public IStorageOperation DeletionForEntity(object entity)
        {
            throw new NotSupportedException("You cannot delete events at this time");
        }

        public IStorageOperation DeletionForWhere(IWhereFragment @where)
        {
            throw new NotSupportedException("You cannot delete events at this time");
        }

        public bool UseOptimisticConcurrency { get; } = false;

        string ISelectClause.FromObject => _tableName;

        Type ISelectClause.SelectedType => typeof(T);

        void ISelectClause.WriteSelectClause(CommandBuilder sql)
        {
            sql.Append("select data from ");
            sql.Append(_tableName);
            sql.Append(" as d");
        }

        ISelector ISelectClause.BuildSelector(IMartenSession session)
        {
            return new EventSelector<T>(session.Serializer);
        }

        IQueryHandler<TResult> ISelectClause.BuildHandler<TResult>(IMartenSession session, Statement topStatement, Statement currentStatement)
        {
            var selector = new EventSelector<T>(session.Serializer);

            return LinqHandlerBuilder.BuildHandler<T, TResult>(selector, topStatement);
        }

        internal class EventSelector<TEvent>: ISelector<TEvent>
        {
            private readonly ISerializer _serializer;

            public EventSelector(ISerializer serializer)
            {
                _serializer = serializer;
            }

            public TEvent Resolve(DbDataReader reader)
            {
                using var json = reader.GetTextReader(0);
                return _serializer.FromJson<TEvent>(json);
            }

            public Task<TEvent> ResolveAsync(DbDataReader reader, CancellationToken token)
            {
                using var json = reader.GetTextReader(0);
                var doc = _serializer.FromJson<TEvent>(json);

                return Task.FromResult(doc);
            }
        }

        ISelectClause ISelectClause.UseStatistics(QueryStatistics statistics)
        {
            throw new NotImplementedException();
        }

        Type IDocumentStorage.SourceType => typeof(IEvent);

        IFieldMapping IDocumentStorage.Fields => this;

        IQueryableDocument IDocumentStorage.QueryableDocument => this;

        object IDocumentStorage<T>.IdentityFor(T document)
        {
            throw new NotImplementedException();
        }

        Type IDocumentStorage<T>.IdType => _idType;

        Guid? IDocumentStorage<T>.VersionFor(T document, IMartenSession session)
        {
            throw new NotImplementedException();
        }

        void IDocumentStorage<T>.Store(IMartenSession session, T document)
        {
            throw new NotImplementedException();
        }

        void IDocumentStorage<T>.Store(IMartenSession session, T document, Guid? version)
        {
            throw new NotImplementedException();
        }

        void IDocumentStorage<T>.Eject(IMartenSession session, T document)
        {
            throw new NotImplementedException();
        }

        IStorageOperation IDocumentStorage<T>.Update(T document, IMartenSession session, ITenant tenant)
        {
            throw new NotImplementedException();
        }

        IStorageOperation IDocumentStorage<T>.Insert(T document, IMartenSession session, ITenant tenant)
        {
            throw new NotImplementedException();
        }

        IStorageOperation IDocumentStorage<T>.Upsert(T document, IMartenSession session, ITenant tenant)
        {
            throw new NotImplementedException();
        }

        IStorageOperation IDocumentStorage<T>.Overwrite(T document, IMartenSession session, ITenant tenant)
        {
            throw new NotImplementedException();
        }

        IStorageOperation IDocumentStorage<T>.DeleteForDocument(T document)
        {
            throw new NotImplementedException();
        }

        IStorageOperation IDocumentStorage<T>.DeleteForWhere(IWhereFragment @where)
        {
            throw new NotImplementedException();
        }
    }
}
