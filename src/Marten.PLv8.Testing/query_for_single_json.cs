﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Marten.Services;
using Marten.Testing.Harness;
using Marten.Util;
using Shouldly;
using Xunit;

namespace Marten.Testing.Linq
{
    public class query_for_single_json : IntegrationContext
    {

        [Fact]
        public void single_returns_only_match()
        {
            var user0 = new SimpleUser
            {
                UserName = "Invisible man",
                Number = 4,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "0", Street = "rue de l'invisible" }
            };
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user0, user1);
            theSession.SaveChanges();

            var userJson = theSession.Query<SimpleUser>().Where(x => x.Number == 5).AsJson().Single();
            userJson.ShouldBeSemanticallySameJsonAs($@"{user1.ToJson()}");
        }

        [Fact]
        public void single_returns_first_and_only()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1);
            theSession.SaveChanges();

            var userJson = theSession.Query<SimpleUser>().AsJson().Single();
            userJson.ShouldBeSemanticallySameJsonAs($@"{user1.ToJson()}");
        }

        [Fact]
        public void single_throws_when_none_found()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            var user2 = new SimpleUser
            {
                UserName = "Mrs Fouine",
                Number = 5,
                Birthdate = new DateTime(1987, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1, user2);
            theSession.SaveChanges();

            var ex = Exception<InvalidOperationException>.ShouldBeThrownBy(() => theSession.Query<SimpleUser>().Where(x => x.Number != 5).AsJson().Single());
            ex.Message.ShouldBe("Sequence contains no elements");
        }

        [Fact]
        public void single_throws_when_more_than_one_found()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            var user2 = new SimpleUser
            {
                UserName = "Mrs Fouine",
                Number = 5,
                Birthdate = new DateTime(1987, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1, user2);
            theSession.SaveChanges();

            var ex = Exception<InvalidOperationException>.ShouldBeThrownBy(() => theSession.Query<SimpleUser>().Where(x => x.Number == 5).AsJson().Single());
            ex.Message.ShouldBe("Sequence contains more than one element");
        }

        [Fact]
        public async Task single_async_returns_only_match()
        {
            var user0 = new SimpleUser
            {
                UserName = "Invisible man",
                Number = 4,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "0", Street = "rue de l'invisible" }
            };
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user0, user1);
            theSession.SaveChanges();

            var userJson = await theSession.Query<SimpleUser>().Where(x => x.Number == 5).AsJson().SingleAsync();
            userJson.ShouldBeSemanticallySameJsonAs($@"{user1.ToJson()}");
        }

        [Fact]
        public async Task single_async_returns_first_and_only()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };

            theSession.Store(user1);
            theSession.SaveChanges();

            var userJson = await theSession.Query<SimpleUser>().AsJson().SingleAsync();
            userJson.ShouldBeSemanticallySameJsonAs($@"{user1.ToJson()}");
        }

        [Fact]
        public async Task single_async_throws_when_none_returned()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            var user2 = new SimpleUser
            {
                UserName = "Mrs Fouine",
                Number = 5,
                Birthdate = new DateTime(1987, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1, user2);
            theSession.SaveChanges();

            var ex = await Exception<InvalidOperationException>.ShouldBeThrownByAsync(() =>
                theSession.Query<SimpleUser>().Where(x => x.Number != 5).AsJson().SingleAsync());
            ex.Message.ShouldBe("Sequence contains no elements");
        }

        [Fact]
        public async Task single_async_throws_when_more_than_one_returned()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            var user2 = new SimpleUser
            {
                UserName = "Mrs Fouine",
                Number = 5,
                Birthdate = new DateTime(1987, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1, user2);
            theSession.SaveChanges();

            var ex = await Exception<InvalidOperationException>.ShouldBeThrownByAsync(() =>
                theSession.Query<SimpleUser>().Where(x => x.Number == 5).AsJson().SingleAsync());
            ex.Message.ShouldBe("Sequence contains more than one element");
        }

        [Fact]
        public void single_or_default_returns_first()
        {
            var user0 = new SimpleUser
            {
                UserName = "Invisible man",
                Number = 4,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "0", Street = "rue de l'invisible" }
            };
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user0, user1);
            theSession.SaveChanges();

            var userJson = theSession.Query<SimpleUser>().Where(x => x.Number == 5).AsJson().SingleOrDefault();
            userJson.ShouldBeSemanticallySameJsonAs($@"{user1.ToJson()}");
        }

        [Fact]
        public void single_or_default_returns_first_and_only_line()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1);
            theSession.SaveChanges();

            var userJson = theSession.Query<SimpleUser>().AsJson().SingleOrDefault();
            userJson.ShouldBeSemanticallySameJsonAs($@"{user1.ToJson()}");
        }

        [Fact]
        public async Task single_or_default_returns_first_async()
        {
            var user0 = new SimpleUser
            {
                UserName = "Invisible man",
                Number = 4,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "0", Street = "rue de l'invisible" }
            };
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            var user2 = new SimpleUser
            {
                UserName = "Mrs Fouine",
                Number = 6,
                Birthdate = new DateTime(1987, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user0, user1, user2);
            theSession.SaveChanges();

            var userJson = await theSession.Query<SimpleUser>().Where(x => x.Number == 5).AsJson().SingleOrDefaultAsync();
            userJson.ShouldBeSemanticallySameJsonAs($@"{user1.ToJson()}");
        }

        [Fact]
        public async Task single_or_default_returns_first_line_async()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1);
            theSession.SaveChanges();

            var userJson = await theSession.Query<SimpleUser>().AsJson().SingleOrDefaultAsync();
            userJson.ShouldBeSemanticallySameJsonAs($@"{user1.ToJson()}");
        }

        [Fact]
        public void single_or_default_returns_default_when_none_found()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            var user2 = new SimpleUser
            {
                UserName = "Mrs Fouine",
                Number = 5,
                Birthdate = new DateTime(1987, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1, user2);
            theSession.SaveChanges();

            var userJson = theSession.Query<SimpleUser>().Where(x => x.Number != 5).AsJson().SingleOrDefault();
            userJson.ShouldBeNull();
        }

        [Fact]
        public void single_or_default_throws_when_more_than_one_found()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            var user2 = new SimpleUser
            {
                UserName = "Mrs Fouine",
                Number = 5,
                Birthdate = new DateTime(1987, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1, user2);
            theSession.SaveChanges();

            var ex = Exception<InvalidOperationException>.ShouldBeThrownBy(() => theSession.Query<SimpleUser>().Where(x => x.Number == 5).AsJson().SingleOrDefault());
            ex.Message.ShouldBe("Sequence contains more than one element");
        }

        [Fact]
        public async Task single_or_default_async_returns_default_when_none_found()
        {
            var user1 = new SimpleUser
            {
                UserName = "Mr Fouine",
                Number = 5,
                Birthdate = new DateTime(1986, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            var user2 = new SimpleUser
            {
                UserName = "Mrs Fouine",
                Number = 5,
                Birthdate = new DateTime(1987, 10, 4),
                Address = new Address { HouseNumber = "12bis", Street = "rue de la martre" }
            };
            theSession.Store(user1, user2);
            theSession.SaveChanges();

            var userJson = await theSession.Query<SimpleUser>().Where(x => x.Number != 5).AsJson().SingleOrDefaultAsync();
            userJson.ShouldBeNull();
        }

        public query_for_single_json(DefaultStoreFixture fixture) : base(fixture)
        {
        }
    }
}
