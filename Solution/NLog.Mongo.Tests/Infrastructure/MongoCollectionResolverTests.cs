namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Moq;

    [TestFixture]
    public class MongoCollectionResolverTests
    {
        private Mock<MongoCollectionResolver.ICacheKeyFactory> _cacheKeyFactory;
        private Mock<MongoCollectionResolver.ICollectionCreator> _collectionCreator;
        private Mock<MongoCollectionResolver.IMongoDatabaseFactory> _databaseFactory;
        private MockRepository _mockFactory;


        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _cacheKeyFactory = _mockFactory.Create<MongoCollectionResolver.ICacheKeyFactory>();
            _collectionCreator = _mockFactory.Create<MongoCollectionResolver.ICollectionCreator>();
            _databaseFactory = _mockFactory.Create<MongoCollectionResolver.IMongoDatabaseFactory>();
        }

        private MongoCollectionResolver Create()
        {
            return new MongoCollectionResolver(_cacheKeyFactory.Object, _collectionCreator.Object, _databaseFactory.Object);
        }

        [Test]
        public void GetCollectionNullArgumentTest() => Assert.Throws<ArgumentNullException>(() => Create().GetCollection(null));

        [Test]
        public void GetCollectionTest()
        {
            var mongosettings = _mockFactory.Create<IMongoSettings>();
            var mongoDb = _mockFactory.Create<IMongoDatabase>();
            var collection = _mockFactory.Create<IMongoCollection<BsonDocument>>();
            const string conStr = "NLog.Mongo.Infrastructure.MongoCollectionResolverTests::GetCollectionTest";
            mongosettings.Setup(x => x.ConnectionString).Returns(conStr).Verifiable();
            _cacheKeyFactory.Setup(x => x.Create(mongosettings.Object)).Returns(
                "NLog.Mongo.Infrastructure.MongoCollectionResolverTests::GetCollectionTest_key").Verifiable();
            _databaseFactory.Setup(x => x.Create(conStr)).Returns(mongoDb.Object).Verifiable();
            _collectionCreator.Setup(x => x.CheckAndCreate(mongosettings.Object, mongoDb.Object))
                              .Returns(Task.FromResult(collection.Object))
                              .Verifiable();
            var actual = Create().GetCollection(mongosettings.Object);
            Assert.AreEqual(collection.Object, actual);
        }

        [Test]
        public void GetCollectionNullDbTest()
        {
            var mongosettings = _mockFactory.Create<IMongoSettings>();
            const string conStr = "NLog.Mongo.Infrastructure.MongoCollectionResolverTests::GetCollectionNullDbTest";
            mongosettings.Setup(x => x.ConnectionString).Returns(conStr).Verifiable();
            _cacheKeyFactory.Setup(x => x.Create(mongosettings.Object)).Returns(
                "NLog.Mongo.Infrastructure.MongoCollectionResolverTests::GetCollectionNullDbTest_key").Verifiable();
            _databaseFactory.Setup(x => x.Create(conStr)).Returns(() => null).Verifiable();
            try
            {
                Create().GetCollection(mongosettings.Object);
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}