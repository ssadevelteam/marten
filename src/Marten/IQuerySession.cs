using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marten.Linq;
using Marten.Schema;
using Marten.Services.BatchQuerying;
using Marten.Storage;
using Marten.Storage.Metadata;
using Npgsql;
#nullable enable
namespace Marten
{
    public interface IQuerySession: IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Find or load a single document of type T by a string id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T? Load<T>(string id) where T : notnull;

        /// <summary>
        /// Asynchronously find or load a single document of type T by a string id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<T?> LoadAsync<T>(string id, CancellationToken token = default) where T : notnull;

        /// <summary>
        /// Load or find a single document of type T with either a numeric or Guid id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T? Load<T>(int id) where T : notnull;

        /// <summary>
        /// Load or find a single document of type T with either a numeric or Guid id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T? Load<T>(long id) where T : notnull;

        /// <summary>
        /// Load or find a single document of type T with either a numeric or Guid id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T? Load<T>(Guid id) where T : notnull;

        /// <summary>
        /// Asynchronously load or find a single document of type T with either a numeric or Guid id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<T?> LoadAsync<T>(int id, CancellationToken token = default) where T : notnull;

        /// <summary>
        /// Asynchronously load or find a single document of type T with either a numeric or Guid id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<T?> LoadAsync<T>(long id, CancellationToken token = default) where T : notnull;

        /// <summary>
        /// Asynchronously load or find a single document of type T with either a numeric or Guid id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<T?> LoadAsync<T>(Guid id, CancellationToken token = default) where T : notnull;

        #region sample_querying_with_linq
        /// <summary>
        /// Use Linq operators to query the documents
        /// stored in Postgresql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IMartenQueryable<T> Query<T>();

        #endregion sample_querying_with_linq

        /// <summary>
        /// Queries the document storage table for the document type T by supplied SQL. See http://jasperfx.github.io/marten/documentation/documents/querying/sql/ for more information on usage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IReadOnlyList<T> Query<T>(string sql, params object[] parameters);

        /// <summary>
        /// Asynchronously queries the document storage table for the document type T by supplied SQL. See http://jasperfx.github.io/marten/documentation/documents/querying/sql/ for more information on usage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="token"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IReadOnlyList<T>> QueryAsync<T>(string sql, CancellationToken token = default, params object[] parameters);

        /// <summary>
        /// Define a batch of deferred queries and load operations to be conducted in one asynchronous request to the
        /// database for potentially performance
        /// </summary>
        /// <returns></returns>
        IBatchedQuery CreateBatchQuery();

        /// <summary>
        /// The currently open Npgsql connection for this session. Use with caution.
        /// </summary>
        NpgsqlConnection Connection { get; }

        /// <summary>
        /// The session specific logger for this session. Can be set for better integration
        /// with custom diagnostics
        /// </summary>
        IMartenSessionLogger Logger { get; set; }

        /// <summary>
        /// Request count
        /// </summary>
        int RequestCount { get; }

        /// <summary>
        /// The document store that created this session
        /// </summary>
        IDocumentStore DocumentStore { get; }

        /// <summary>
        /// A query that is compiled so a copy of the DbCommand can be used directly in subsequent requests.
        /// </summary>
        /// <typeparam name="TDoc">The document</typeparam>
        /// <typeparam name="TOut">The output</typeparam>
        /// <param name="query">The instance of a compiled query</param>
        /// <returns>A single item query result</returns>
        TOut Query<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query);

        /// <summary>
        /// An async query that is compiled so a copy of the DbCommand can be used directly in subsequent requests.
        /// </summary>
        /// <typeparam name="TDoc">The document</typeparam>
        /// <typeparam name="TOut">The output</typeparam>
        /// <param name="query">The instance of a compiled query</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>A task for a single item query result</returns>
        Task<TOut> QueryAsync<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, CancellationToken token = default);

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyList<T> LoadMany<T>(params string[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyList<T> LoadMany<T>(IEnumerable<string> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyList<T> LoadMany<T>(params Guid[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyList<T> LoadMany<T>(IEnumerable<Guid> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyList<T> LoadMany<T>(params int[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyList<T> LoadMany<T>(IEnumerable<int> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyList<T> LoadMany<T>(params long[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyList<T> LoadMany<T>(IEnumerable<long> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(params string[] ids) where T : notnull; 

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<string> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(params Guid[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<Guid> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(params int[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<int> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(params long[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<long> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params string[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<string> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params Guid[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<Guid> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params int[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<int> ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params long[] ids) where T : notnull;

        /// <summary>
        /// Load or find multiple documents by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<long> ids) where T : notnull;

        /// <summary>
        /// Directly load the persisted JSON data for documents by Id
        /// </summary>
        IJsonLoader Json { get; }

        /// <summary>
        /// Optional metadata describing the causation id for this
        /// unit of work
        /// </summary>
        string? CausationId { get; set; }

        /// <summary>
        /// Optional metadata describing the correlation id for this
        /// unit of work
        /// </summary>
        string? CorrelationId { get; set; }

        /// <summary>
        /// Writeable list of the interceptors for this session
        /// </summary>
        IList<IDbCommandInterceptor> Interceptors { get; }

        /// <summary>
        /// Retrieve the current known version of the given document
        /// according to this session. Will return null if the document is
        /// not part of this session
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Guid? VersionFor<TDoc>(TDoc entity) where TDoc : notnull;

        /// <summary>
        /// Performs a full text search against <typeparamref name="TDoc"/>
        /// </summary>
        /// <param name="queryText">The text to search for.  May contain lexeme patterns used by PostgreSQL for full text searching</param>
        /// <param name="regConfig">The dictionary config passed to the 'to_tsquery' function, must match the config parameter used by <seealso cref="DocumentMapping.AddFullTextIndex(string)"/></param>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/10/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </remarks>
        IReadOnlyList<TDoc> Search<TDoc>(string queryText, string regConfig = FullTextIndex.DefaultRegConfig);

        /// <summary>
        /// Performs an asynchronous full text search against <typeparamref name="TDoc"/>
        /// </summary>
        /// <param name="queryText">The text to search for.  May contain lexeme patterns used by PostgreSQL for full text searching</param>
        /// <param name="regConfig">The dictionary config passed to the 'to_tsquery' function, must match the config parameter used by <seealso cref="DocumentMapping.AddFullTextIndex(string)"/></param>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/10/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </remarks>
        Task<IReadOnlyList<TDoc>> SearchAsync<TDoc>(string queryText, string regConfig = FullTextIndex.DefaultRegConfig, CancellationToken token = default);

        /// <summary>
        /// Performs a full text search against <typeparamref name="TDoc"/> using the 'plainto_tsquery' search function
        /// </summary>
        /// <param name="queryText">The text to search for.  May contain lexeme patterns used by PostgreSQL for full text searching</param>
        /// <param name="regConfig">The dictionary config passed to the 'to_tsquery' function, must match the config parameter used by <seealso cref="DocumentMapping.AddFullTextIndex(string)"/></param>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/10/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </remarks>
        IReadOnlyList<TDoc> PlainTextSearch<TDoc>(string searchTerm, string regConfig = FullTextIndex.DefaultRegConfig);

        /// <summary>
        /// Performs an asynchronous full text search against <typeparamref name="TDoc"/> using the 'plainto_tsquery' function
        /// </summary>
        /// <param name="queryText">The text to search for.  May contain lexeme patterns used by PostgreSQL for full text searching</param>
        /// <param name="regConfig">The dictionary config passed to the 'to_tsquery' function, must match the config parameter used by <seealso cref="DocumentMapping.AddFullTextIndex(string)"/></param>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/10/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </remarks>
        Task<IReadOnlyList<TDoc>> PlainTextSearchAsync<TDoc>(string searchTerm, string regConfig = FullTextIndex.DefaultRegConfig, CancellationToken token = default);

        /// <summary>
        /// Performs a full text search against <typeparamref name="TDoc"/> using the 'phraseto_tsquery' search function
        /// </summary>
        /// <param name="queryText">The text to search for.  May contain lexeme patterns used by PostgreSQL for full text searching</param>
        /// <param name="regConfig">The dictionary config passed to the 'to_tsquery' function, must match the config parameter used by <seealso cref="DocumentMapping.AddFullTextIndex(string)"/></param>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/10/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </remarks>
        IReadOnlyList<TDoc> PhraseSearch<TDoc>(string searchTerm, string regConfig = FullTextIndex.DefaultRegConfig);

        /// <summary>
        /// Performs an asynchronous full text search against <typeparamref name="TDoc"/> using the 'phraseto_tsquery' search function
        /// </summary>
        /// <param name="queryText">The text to search for.  May contain lexeme patterns used by PostgreSQL for full text searching</param>
        /// <param name="regConfig">The dictionary config passed to the 'to_tsquery' function, must match the config parameter used by <seealso cref="DocumentMapping.AddFullTextIndex(string)"/></param>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/10/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </remarks>
        Task<IReadOnlyList<TDoc>> PhraseSearchAsync<TDoc>(string searchTerm, string regConfig = FullTextIndex.DefaultRegConfig, CancellationToken token = default);

        /// <summary>
        /// Performs a full text search against <typeparamref name="TDoc"/> using the 'websearch_to_tsquery' search function
        /// </summary>
        /// <param name="searchTerm">The text to search for.  Uses an alternative syntax to the other search functions, similar to the one used by web search engines</param>
        /// <param name="regConfig">The dictionary config passed to the 'websearch_to_tsquery' function, must match the config parameter used by <seealso cref="DocumentMapping.AddFullTextIndex(string)"/></param>
        /// <remarks>
        /// Supported from Postgres 11
        /// See: https://www.postgresql.org/docs/11/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </remarks>
        IReadOnlyList<TDoc> WebStyleSearch<TDoc>(string searchTerm, string regConfig = FullTextIndex.DefaultRegConfig);

        /// <summary>
        /// Performs an asynchronous full text search against <typeparamref name="TDoc"/> using the 'websearch_to_tsquery' search function
        /// </summary>
        /// <param name="searchTerm">The text to search for.  Uses an alternative syntax to the other search functions, similar to the one used by web search engines</param>
        /// <param name="regConfig">The dictionary config passed to the 'websearch_to_tsquery' function, must match the config parameter used by <seealso cref="DocumentMapping.AddFullTextIndex(string)"/></param>
        /// <param name="token"></param>
        /// <remarks>
        /// Supported from Postgres 11
        /// See: https://www.postgresql.org/docs/11/static/textsearch-controls.html#TEXTSEARCH-PARSING-QUERIES
        /// </remarks>
        Task<IReadOnlyList<TDoc>> WebStyleSearchAsync<TDoc>(string searchTerm, string regConfig = FullTextIndex.DefaultRegConfig, CancellationToken token = default);


        /// <summary>
        ///     Fetch the entity version and last modified time from the database
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DocumentMetadata MetadataFor<T>(T entity) where T : notnull;

        /// <summary>
        ///     Fetch the entity version and last modified time from the database
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<DocumentMetadata> MetadataForAsync<T>(T entity,
            CancellationToken token = default) where T : notnull;
    }
}
