namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Moq;

    [TestFixture]
    public class CollectionCreatorTests
    {
        private MockRepository _mockFactory;
        private Mock<IMongoSettings> _settings;
        private Mock<IMongoDatabase> _db;

        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _settings = _mockFactory.Create<IMongoSettings>();
            _db = _mockFactory.Create<IMongoDatabase>();
        }

        private MongoCollectionResolver.CollectionCreator Create()
        {
            return new MongoCollectionResolver.CollectionCreator();
        }

        [Test]
        public void CreateNullArg1Test() => Assert.ThrowsAsync<ArgumentNullException>(async () => await Create().CheckAndCreate(null, _db.Object));

        [Test]
        public void CreateNullArg2Test() => Assert.ThrowsAsync<ArgumentNullException>(async () => await Create().CheckAndCreate(_settings.Object, null));

        [Test]
        public async Task CreateTest()
        {
            var cursor = _mockFactory.Create<IAsyncCursor<BsonDocument>>();
            cursor.Setup(x => x.Dispose()).Verifiable();
            cursor.Setup(x => x.MoveNextAsync(default(CancellationToken))).Returns(() => Task.FromResult(true));
            cursor.Setup(x => x.Current).Returns(() => new BsonDocument[0]).Verifiable();
            const string collectionName = "Collection";
            
            _settings.Setup(x => x.CollectionName).Returns(collectionName).Verifiable();
            _settings.Setup(x => x.CappedCollectionSize).Returns(100).Verifiable();
            _settings.Setup(x => x.CappedCollectionMaxItems).Returns(1000).Verifiable();
            _db.Setup(x => x.ListCollectionsAsync(It.IsAny<ListCollectionsOptions>(), default(CancellationToken))).Returns((ListCollectionsOptions options, CancellationToken t) =>
            {
                Assert.IsNotNull(options.Filter);
                var f = options.Filter as BsonDocumentFilterDefinition<BsonDocument>;
                Assert.IsNotNull(f);
                var bsonValue = f.Document["name"];
                Assert.AreEqual(bsonValue.AsString, collectionName);
                return Task.FromResult(cursor.Object);
            }).Verifiable();

            

            _db.Setup(x => x.CreateCollectionAsync(collectionName, It.IsAny<CreateCollectionOptions>(), default(CancellationToken)))
                .Returns((string s, CreateCollectionOptions options, CancellationToken t) =>
                {
                    Assert.IsNotNull(options);
                    Assert.IsNotNull(options.Capped);
                    Assert.IsTrue(options.Capped.Value);
                    Assert.AreEqual(options.MaxSize, 100);
                    Assert.AreEqual(options.MaxDocuments, 1000);
                    return Task.CompletedTask;
                }).Verifiable();
            var collection = _mockFactory.Create<IMongoCollection<BsonDocument>>();
            _db.Setup(x => x.GetCollection<BsonDocument>(collectionName, null)).Returns(collection.Object).Verifiable();
            var actual = await Create().CheckAndCreate(_settings.Object, _db.Object);
            Assert.AreEqual(collection.Object, actual);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}