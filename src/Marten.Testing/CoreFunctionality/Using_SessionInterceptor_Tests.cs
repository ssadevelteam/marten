using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten.Services;
using Marten.Testing.Documents;
using Marten.Testing.Harness;
using Npgsql;
using Xunit;

namespace Marten.Testing.CoreFunctionality
{
    // todo: add tests for the NonQueryExecuting and NonQueryExecutingAsync method calls
    [Collection("interceptors")]
    public class Using_SessionInterceptor_Tests : OneOffConfigurationsContext
    {
        public Using_SessionInterceptor_Tests() : base("interceptors")
        {
        }

        [Fact]
        public void call_interceptor_events_on_synchronous_connection_ReaderExecuting()
        {
            var stub1 = new StubDbCommandInterceptor();
            var stub2 = new StubDbCommandInterceptor();

            using var store = SeparateStore(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
            store.Advanced.Clean.CompletelyRemoveAll();

            using var session = store.OpenSession(new SessionOptions { Interceptors = { stub1, stub2 } });

            session.Store(new User(), new User());

            session.SaveChanges();

            Assert.Empty(stub1.NonQueryExecutingCommands);
            Assert.Empty(stub1.NonQueryExecutingAsyncCommands);
            Assert.Single(stub1.ReaderExecutingCommands);
            Assert.Empty(stub1.ReaderExecutingAsyncCommands);

            Assert.Empty(stub2.NonQueryExecutingCommands);
            Assert.Empty(stub2.NonQueryExecutingAsyncCommands);
            Assert.Single(stub2.ReaderExecutingCommands);
            Assert.Empty(stub2.ReaderExecutingAsyncCommands);
        }

        [Fact]
        public async Task call_interceptor_events_on_synchronous_connection_ReaderExecutingAsync()
        {
            var stub1 = new StubDbCommandInterceptor();
            var stub2 = new StubDbCommandInterceptor();

            using var store = SeparateStore(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
            store.Advanced.Clean.CompletelyRemoveAll();

            await using var session = store.OpenSession(new SessionOptions { Interceptors = { stub1, stub2 } });
            {
                session.Store(new User(), new User());

                await session.SaveChangesAsync();

                Assert.Empty(stub1.NonQueryExecutingCommands);
                Assert.Empty(stub1.NonQueryExecutingAsyncCommands);
                Assert.Empty(stub1.ReaderExecutingCommands);
                Assert.Single(stub1.ReaderExecutingAsyncCommands);

                Assert.Empty(stub2.NonQueryExecutingCommands);
                Assert.Empty(stub2.NonQueryExecutingAsyncCommands);
                Assert.Empty(stub2.ReaderExecutingCommands);
                Assert.Single(stub2.ReaderExecutingAsyncCommands);
            }
        }

        [Fact]
        public void call_interceptor_events_on_document_store_do_not_get_used()
        {
            var stub1 = new StubDbCommandInterceptor();
            var stub2 = new StubDbCommandInterceptor();

            using (var store = SeparateStore(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            }))
            {
                store.Advanced.Clean.CompletelyRemoveAll();

                using var session = store.OpenSession(new SessionOptions { Interceptors = { stub1, stub2 } });
                var user1 = new User { Id = Guid.NewGuid() };
                var user2 = new User { Id = Guid.NewGuid() };

                session.Store(user1, user2);

                Assert.Empty(stub1.NonQueryExecutingCommands);
                Assert.Empty(stub1.NonQueryExecutingAsyncCommands);
                Assert.Empty(stub1.ReaderExecutingCommands);
                Assert.Empty(stub1.ReaderExecutingAsyncCommands);

                Assert.Empty(stub2.NonQueryExecutingCommands);
                Assert.Empty(stub2.NonQueryExecutingAsyncCommands);
                Assert.Empty(stub2.ReaderExecutingCommands);
                Assert.Empty(stub2.ReaderExecutingAsyncCommands);
            }
        }


        [Fact]
        public void call_interceptor_events_on_document_load()
        {
            var stub1 = new StubDbCommandInterceptor();
            var stub2 = new StubDbCommandInterceptor();

            using var store = SeparateStore(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
            store.Advanced.Clean.CompletelyRemoveAll();

            var user1 = new User { Id = Guid.NewGuid() };
            var user2 = new User { Id = Guid.NewGuid() };

            using (var session = store.OpenSession())
            {
                session.StoreObjects(new[] { user1, user2 });
                session.SaveChanges();
            }

            using (var session = store.OpenSession(new SessionOptions { Interceptors = { stub1, stub2 } }))
            {
                var user = session.Load<User>(user1.Id);

                Assert.Empty(stub1.NonQueryExecutingCommands);
                Assert.Empty(stub1.NonQueryExecutingAsyncCommands);
                Assert.Single(stub1.ReaderExecutingCommands);
                Assert.Empty(stub1.ReaderExecutingAsyncCommands);

                Assert.Empty(stub2.NonQueryExecutingCommands);
                Assert.Empty(stub2.NonQueryExecutingAsyncCommands);
                Assert.Single(stub2.ReaderExecutingCommands);
                Assert.Empty(stub2.ReaderExecutingAsyncCommands);
            }
        }

        [Fact]
        public void call_interceptor_events_on_document_query()
        {
            var stub1 = new StubDbCommandInterceptor();
            var stub2 = new StubDbCommandInterceptor();

            using var store = SeparateStore(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
            store.Advanced.Clean.CompletelyRemoveAll();

            var user1 = new User { Id = Guid.NewGuid() };
            var user2 = new User { Id = Guid.NewGuid() };

            using (var session = store.OpenSession())
            {
                session.StoreObjects(new[] { user1, user2 });
                session.SaveChanges();
            }

            using (var session = store.OpenSession(new SessionOptions { Interceptors = { stub1, stub2 } }))
            {
                var users = session.Query<User>().ToList();

                Assert.Empty(stub1.NonQueryExecutingCommands);
                Assert.Empty(stub1.NonQueryExecutingAsyncCommands);
                Assert.Single(stub1.ReaderExecutingCommands);
                Assert.Empty(stub1.ReaderExecutingAsyncCommands);

                Assert.Empty(stub2.NonQueryExecutingCommands);
                Assert.Empty(stub2.NonQueryExecutingAsyncCommands);
                Assert.Single(stub2.ReaderExecutingCommands);
                Assert.Empty(stub2.ReaderExecutingAsyncCommands);
            }
        }

        [Fact]
        public void call_interceptor_events_on_document_store_and_dirty_tracking_session()
        {
            var stub1 = new StubDbCommandInterceptor();
            var stub2 = new StubDbCommandInterceptor();

            using var store = SeparateStore(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
            store.Advanced.Clean.CompletelyRemoveAll();

            using var session = store.OpenSession(new SessionOptions { Tracking = DocumentTracking.DirtyTracking, Interceptors = { stub1, stub2 } });
            var user1 = new User { Id = Guid.NewGuid() };
            var user2 = new User { Id = Guid.NewGuid() };

            session.Store(user1, user2);
            session.SaveChanges();

            Assert.Empty(stub1.NonQueryExecutingCommands);
            Assert.Empty(stub1.NonQueryExecutingAsyncCommands);
            Assert.Single(stub1.ReaderExecutingCommands);
            Assert.Empty(stub1.ReaderExecutingAsyncCommands);

            Assert.Empty(stub2.NonQueryExecutingCommands);
            Assert.Empty(stub2.NonQueryExecutingAsyncCommands);
            Assert.Single(stub2.ReaderExecutingCommands);
            Assert.Empty(stub2.ReaderExecutingAsyncCommands);
        }

        [Fact]
        public void call_interceptor_events_on_document_store_objects_and_dirty_tracking_session()
        {
            var stub1 = new StubDbCommandInterceptor();
            var stub2 = new StubDbCommandInterceptor();

            using var store = SeparateStore(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
            store.Advanced.Clean.CompletelyRemoveAll();

            using var session = store.OpenSession(new SessionOptions { Tracking = DocumentTracking.DirtyTracking, Interceptors = { stub1, stub2 } });
            var user1 = new User { Id = Guid.NewGuid() };
            var user2 = new User { Id = Guid.NewGuid() };

            session.StoreObjects(new[] { user1, user2 });
            session.SaveChanges();

            Assert.Empty(stub1.NonQueryExecutingCommands);
            Assert.Empty(stub1.NonQueryExecutingAsyncCommands);
            Assert.Single(stub1.ReaderExecutingCommands);
            Assert.Empty(stub1.ReaderExecutingAsyncCommands);

            Assert.Empty(stub2.NonQueryExecutingCommands);
            Assert.Empty(stub2.NonQueryExecutingAsyncCommands);
            Assert.Single(stub2.ReaderExecutingCommands);
            Assert.Empty(stub2.ReaderExecutingAsyncCommands);
        }

        [Fact]
        public void call_interceptor_events_on_document_load_and_dirty_tracking_session()
        {
            var stub1 = new StubDbCommandInterceptor();
            var stub2 = new StubDbCommandInterceptor();

            using var store = SeparateStore(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
            store.Advanced.Clean.CompletelyRemoveAll();

            var user1 = new User { Id = Guid.NewGuid() };
            var user2 = new User { Id = Guid.NewGuid() };

            using (var session = store.OpenSession())
            {
                session.StoreObjects(new[] { user1, user2 });
                session.SaveChanges();
            }

            using (var session = store.OpenSession(new SessionOptions { Tracking = DocumentTracking.DirtyTracking, Interceptors = { stub1, stub2 } }))
            {
                var user = session.Load<User>(user1.Id);

                Assert.Empty(stub1.NonQueryExecutingCommands);
                Assert.Empty(stub1.NonQueryExecutingAsyncCommands);
                Assert.Single(stub1.ReaderExecutingCommands);
                Assert.Empty(stub1.ReaderExecutingAsyncCommands);

                Assert.Empty(stub2.NonQueryExecutingCommands);
                Assert.Empty(stub2.NonQueryExecutingAsyncCommands);
                Assert.Single(stub2.ReaderExecutingCommands);
                Assert.Empty(stub2.ReaderExecutingAsyncCommands);
            }
        }

        [Fact]
        public void call_interceptor_events_on_document_query_and_dirty_tracking_session()
        {
            var stub1 = new StubDbCommandInterceptor();
            var stub2 = new StubDbCommandInterceptor();

            using var store = SeparateStore(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
            });
            store.Advanced.Clean.CompletelyRemoveAll();

            var user1 = new User { Id = Guid.NewGuid() };
            var user2 = new User { Id = Guid.NewGuid() };

            using (var session = store.OpenSession())
            {
                session.StoreObjects(new[] { user1, user2 });
                session.SaveChanges();
            }

            using (var session = store.OpenSession(new SessionOptions { Tracking = DocumentTracking.DirtyTracking, Interceptors = { stub1, stub2 } }))
            {
                var users = session.Query<User>().ToList();

                Assert.Empty(stub1.NonQueryExecutingCommands);
                Assert.Empty(stub1.NonQueryExecutingAsyncCommands);
                Assert.Single(stub1.ReaderExecutingCommands);
                Assert.Empty(stub1.ReaderExecutingAsyncCommands);

                Assert.Empty(stub2.NonQueryExecutingCommands);
                Assert.Empty(stub2.NonQueryExecutingAsyncCommands);
                Assert.Single(stub2.ReaderExecutingCommands);
                Assert.Empty(stub2.ReaderExecutingAsyncCommands);
            }
        }
    }

    public class StubDbCommandInterceptor : IDbCommandInterceptor
    {
        public List<NpgsqlCommand> NonQueryExecutingCommands { get; } = new List<NpgsqlCommand>();
        public List<NpgsqlCommand> ReaderExecutingCommands { get; } = new List<NpgsqlCommand>();
        public List<NpgsqlCommand> ReaderExecutingAsyncCommands { get; } = new List<NpgsqlCommand>();
        public List<NpgsqlCommand> NonQueryExecutingAsyncCommands { get; } = new List<NpgsqlCommand>();

        public void NonQueryExecuting(NpgsqlCommand command)
        {
            NonQueryExecutingCommands.Add(command);
        }

        public void ReaderExecuting(NpgsqlCommand command)
        {
            ReaderExecutingCommands.Add(command);
        }

        public Task ReaderExecutingAsync(NpgsqlCommand command)
        {
            ReaderExecutingAsyncCommands.Add(command);
            return Task.CompletedTask;
        }

        public Task NonQueryExecutingAsync(NpgsqlCommand command)
        {
            NonQueryExecutingAsyncCommands.Add(command);
            return Task.CompletedTask;
        }
    }
}
