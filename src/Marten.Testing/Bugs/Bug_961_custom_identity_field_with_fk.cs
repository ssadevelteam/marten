using System;
using System.Threading.Tasks;
using Marten.Testing.Harness;
using Xunit;

namespace Marten.Testing.Bugs
{
    public class Bug_961_custom_identity_field_with_fk: BugIntegrationContext
    {
        public class FkTarget
        {
            public Guid SomeId { get; set; }
        }

        public class Document
        {
            public int Id { get; set; }
            public Guid TargetId { get; set; }
        }

        [Fact]
        public async Task can_build_the_fk_correctly()
        {
            StoreOptions(_ =>
            {
                _.Schema.For<FkTarget>().Identity(x => x.SomeId);

                _.Schema.For<Document>().ForeignKey<FkTarget>(a => a.TargetId);
            });

            await theStore.Schema.ApplyAllConfiguredChangesToDatabase();
        }

    }
}
