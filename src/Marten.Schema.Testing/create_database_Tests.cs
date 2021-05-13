﻿using System;
using System.Threading.Tasks;
using Marten.Schema.Testing.Documents;
using Marten.Testing.Harness;
using Npgsql;
using Shouldly;
using Weasel.Postgresql;
using Xunit;
using Xunit.Sdk;

namespace Marten.Schema.Testing
{
    public class create_database_Tests : IDisposable
    {
        [Fact]
        public async Task can_create_new_database_when_one_does_not_exist_for_default_tenant()
        {
            var cstring = ConnectionSource.ConnectionString;

            TryDropDb(dbName);

            using (var store1 = DocumentStore.For(_ =>
            {
                _.Connection(dbToCreateConnectionString);
            }))
            {
                await Should.ThrowAsync<PostgresException>(async () =>
                {
                    await store1.Schema.ApplyAllConfiguredChangesToDatabase();
                });
            }

            var dbCreated = false;

            using (var store = DocumentStore.For(storeOptions =>
            {
                storeOptions.Connection(dbToCreateConnectionString);
                #region sample_marten_create_database
                storeOptions.CreateDatabasesForTenants(c =>
                {
                    // Specify a db to which to connect in case database needs to be created.
                    // If not specified, defaults to 'postgres' on the connection for a tenant.
                    c.MaintenanceDatabase(cstring);
                    c.ForTenant()
                        .CheckAgainstPgDatabase()
                        .WithOwner("postgres")
                        .WithEncoding("UTF-8")
                        .ConnectionLimit(-1)
                        .OnDatabaseCreated(_ =>
                        {
                            dbCreated = true;
                        });
                });
                #endregion sample_marten_create_database
            }))
            {
                await store.Schema.ApplyAllConfiguredChangesToDatabase();
                await store.Schema.AssertDatabaseMatchesConfiguration();
                Assert.True(dbCreated);
            }
        }

        [Fact]
        public void can_use_existing_database_without_calling_into_create()
        {
            var user1 = new User { FirstName = "User" };
            var dbCreated = false;
            using (var store = DocumentStore.For(_ =>
            {
	            _.AutoCreateSchemaObjects = AutoCreate.All;
                _.Connection(ConnectionSource.ConnectionString);
                _.CreateDatabasesForTenants(c =>
                {
                    c.MaintenanceDatabase(ConnectionSource.ConnectionString);
                    c.ForTenant()
                        .CheckAgainstPgDatabase()
                        .WithOwner("postgres")
                        .WithEncoding("UTF-8")
                        .ConnectionLimit(-1)
                        .OnDatabaseCreated(___ => dbCreated = true);
                });

            }))
            {
                using (var session = store.OpenSession())
                {
                    session.Store(user1);
                    session.SaveChanges();
                }

                Assert.False(dbCreated);
            }
        }

        [Fact]
        public void can_create_new_database_when_one_does_not_exist_with_plv8_extension()
        {
            using (var store = DocumentStore.For(_ =>
            {
                _.Connection(dbToCreateConnectionString);
                _.CreateDatabasesForTenants(c =>
                {
                    c.MaintenanceDatabase(ConnectionSource.ConnectionString);
                    c.ForTenant()
                        .CheckAgainstPgDatabase()
                        .CreatePLV8()
                        .WithOwner("postgres");
                });

            }))
            {
                using (var connection = store.Tenancy.Default.CreateConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT extname FROM pg_extension";
                    NpgsqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        if (reader["extname"].ToString().ToLowerInvariant() == "plv8")
                        {
                            return;
                        }
                    }
                    connection.Close();
                }

            }

            throw new XunitException("Expected plv8 extension created");
        }

        private readonly string dbToCreateConnectionString;
        private readonly string dbName;

        private static Tuple<string, string> DbToCreate(string cstring)
        {
            var builder = new NpgsqlConnectionStringBuilder(cstring);
            builder.Database = $"_dropme{DateTime.UtcNow.Ticks}_{builder.Database}";
            return Tuple.Create(builder.ToString(), builder.Database);
        }

        public create_database_Tests()
        {
            var db = DbToCreate(ConnectionSource.ConnectionString);
            dbToCreateConnectionString = db.Item1;
            dbName = db.Item2;
        }

        private static bool TryDropDb(string db)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionSource.ConnectionString))
                using (var cmd = connection.CreateCommand())
                {
                    try
                    {
                        connection.Open();
                        // Ensure connections to DB are killed - there seems to be a lingering idle session after AssertDatabaseMatchesConfiguration(), even after store disposal
                        cmd.CommandText +=
                            $"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{db}' AND pid <> pg_backend_pid();";
                        cmd.CommandText += $"DROP DATABASE IF EXISTS {db};";
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            TryDropDb(dbName);
        }
    }
}
