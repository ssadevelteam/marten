using System;
using System.Threading.Tasks;
using Marten.Testing.Documents;
using Marten.Testing.Harness;
using Shouldly;
using Xunit;

namespace Marten.Testing.Acceptance
{
    public class JsTransformsFixture : StoreFixture
    {
        public JsTransformsFixture() : base("js")
        {
            Options.DatabaseSchemaName = "js";

            throw new NotImplementedException();
            //Options.Transforms.LoadFile("default_username.js");
            Options.Schema.For<MultiTenantUser>().MultiTenanted();
        }
    }

    public class MultiTenantUser: User
    {

    }

    public class document_transforms: StoreContext<JsTransformsFixture>, IClassFixture<JsTransformsFixture>
    {
        public document_transforms(JsTransformsFixture fixture) : base(fixture)
        {
            theStore.Advanced.Clean.DeleteAllDocuments();
        }

        public void example()
        {
            #region sample_loading_js_transform_files
            var store = DocumentStore.For(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);

                throw new NotImplementedException();
                // // Let Marten derive the transform name from the filename
                // _.Transforms.LoadFile("get_fullname.js");
                //
                // // Explicitly define the transform name yourself
                // _.Transforms.LoadFile("default_username.js", "set_default_username");
            });
            #endregion sample_loading_js_transform_files

            transform_example(store);

            store.Dispose();
        }

        #region sample_transform_example
        private static void transform_example(IDocumentStore store)
        {
            throw new NotImplementedException();
            // // Transform User documents with a filter
            // store.Transform.Where<User>("default_username", x => x.UserName == null);
            //
            // // Transform a single User document by Id
            // store.Transform.Document<User>("default_username", Guid.NewGuid());
            //
            // // Transform all User documents
            // store.Transform.All<User>("default_username");
            //
            // // Only transform documents from the "tenant1" tenant
            // store.Transform.Tenant<User>("default_username", "tenant1");
            //
            // // Only transform documents from the named tenants
            // store.Transform.Tenants<User>("default_username", "tenant1", "tenant2");
        }

        #endregion sample_transform_example

        [Fact] //-- Unreliable on CI
        public async Task use_transform_in_production_mode()
        {
            theStore.Tenancy.Default.EnsureStorageExists(typeof(User));
            await theStore.Schema.ApplyAllConfiguredChangesToDatabase();

            throw new NotImplementedException();
            //theStore.Transform.All<User>("default_username");

        }

        [Fact]
        public void transform_all_documents()
        {
            var user1 = new User { FirstName = "Jeremy", LastName = "Miller" };
            var user2 = new User { FirstName = "Corey", LastName = "Kaylor" };
            var user3 = new User { FirstName = "Tim", LastName = "Cools" };

            theStore.BulkInsert(new User[] { user1, user2, user3 });

            throw new NotImplementedException();
            //theStore.Transform.All<User>("default_username");

            using (var session = theStore.QuerySession())
            {
                session.Load<User>(user1.Id).UserName.ShouldBe("jeremy.miller");
                session.Load<User>(user2.Id).UserName.ShouldBe("corey.kaylor");
                session.Load<User>(user3.Id).UserName.ShouldBe("tim.cools");
            }
        }

        [Fact]
        public void transform_for_tenants()
        {

            var user1 = new MultiTenantUser() { FirstName = "Jeremy", LastName = "Miller" };
            var user2 = new MultiTenantUser { FirstName = "Corey", LastName = "Kaylor" };
            var user3 = new MultiTenantUser { FirstName = "Tim", LastName = "Cools", UserName = "NotTransformed" };

            theStore.BulkInsert("Purple", new MultiTenantUser[] { user1, user2 });
            theStore.BulkInsert("Orange", new MultiTenantUser[] { user3 });

            throw new NotImplementedException();
            // theStore.Transform.Tenant<MultiTenantUser>("default_username", "Purple");
            //
            // using (var query = theStore.QuerySession("Purple"))
            // {
            //     query.Load<MultiTenantUser>(user1.Id).UserName.ShouldBe("jeremy.miller");
            // }
            //
            // using (var query = theStore.QuerySession("Orange"))
            // {
            //     query.Load<MultiTenantUser>(user3.Id).UserName.ShouldBe("NotTransformed");
            // }
        }

        [Fact]
        public void transform_a_single_document()
        {
            var user1 = new User { FirstName = "Jeremy", LastName = "Miller" };
            var user2 = new User { FirstName = "Corey", LastName = "Kaylor", UserName = "user2" };
            var user3 = new User { FirstName = "Tim", LastName = "Cools", UserName = "user3" };

            theStore.BulkInsert(new User[] { user1, user2, user3 });

            throw new NotImplementedException();
            //theStore.Transform.Document<User>("default_username", user1.Id);

            using (var session = theStore.QuerySession())
            {
                session.Load<User>(user1.Id).UserName.ShouldBe("jeremy.miller");

                // no updates to these
                session.Load<User>(user2.Id).UserName.ShouldBe("user2");
                session.Load<User>(user3.Id).UserName.ShouldBe("user3");
            }
        }

        [Fact]
        public void transform_with_where_clause()
        {
            var user1 = new User { FirstName = "Jeremy", LastName = "Miller" };
            var user2 = new User { FirstName = "Corey", LastName = "Kaylor", UserName = "user2" };
            var user3 = new User { FirstName = "Tim", LastName = "Cools", UserName = "user3" };

            theStore.BulkInsert(new User[] { user1, user2, user3 });

            throw new NotImplementedException();
            //theStore.Transform.Where<User>("default_username", x => x.FirstName == user1.FirstName);

            using (var session = theStore.QuerySession())
            {
                session.Load<User>(user1.Id).UserName.ShouldBe("jeremy.miller");

                // no updates to these
                session.Load<User>(user2.Id).UserName.ShouldBe("user2");
                session.Load<User>(user3.Id).UserName.ShouldBe("user3");
            }
        }
    }
}
