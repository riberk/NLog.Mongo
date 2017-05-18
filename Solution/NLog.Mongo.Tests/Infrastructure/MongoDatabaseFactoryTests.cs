namespace NLog.Mongo.Infrastructure
{
    using System;
    using NUnit.Framework;
    using Moq;

    [TestFixture]
    public class MongoDatabaseFactoryTests
    {
        private MockRepository _mockFactory;

        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
        }

        private MongoCollectionResolver.MongoDatabaseFactory Create()
        {
            return new MongoCollectionResolver.MongoDatabaseFactory();
        }

        [Test]
        public void CreateArgumentNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => Create().Create(null));
        }

        [Test]
        public void CreateTest()
        {
            var db = Create().Create("mongodb://localhost/TestDatabase");
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}