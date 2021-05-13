using System;
using System.Linq;
using Marten.Testing.Documents;
using Marten.Testing.Harness;
using Marten.Testing.Linq;
using Xunit;

namespace Marten.PLv8.Testing.Transforms
{
    public class select_many_with_transform : IntegrationContext
    {
        public select_many_with_transform(DefaultStoreFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void project_select_many_with_javascript()
        {

            throw new NotImplementedException();
            // StoreOptions(_ =>
            // {
            //     _.Transforms.LoadFile("get_target_float.js");
            // });
            //
            // var targets = Target.GenerateRandomData(100).ToArray();
            // theStore.BulkInsert(targets);
            //
            // using (var query = theStore.OpenSession())
            // {
            //     var count = targets
            //         .Where(x => x.Flag)
            //         .SelectMany(x => x.Children)
            //         .Count(x => x.Color == Colors.Green);
            //
            //     var jsonList = query.Query<Target>()
            //         .Where(x => x.Flag)
            //         .SelectMany(x => x.Children)
            //         .Where(x => x.Color == Colors.Green)
            //         .TransformToJson("get_target_float").ToList();
            //
            //     jsonList.Count.ShouldBe(count);
            //
            //     var transformed = query.Query<Target>()
            //         .Where(x => x.Flag)
            //         .SelectMany(x => x.Children)
            //         .Where(x => x.Color == Colors.Green)
            //         .TransformTo<TargetNumbers>("get_target_float")
            //         .ToList();
            //
            //     transformed.Count.ShouldBe(count);
            //}
        }

    }
}
