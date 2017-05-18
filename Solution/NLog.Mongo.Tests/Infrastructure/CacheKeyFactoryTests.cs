namespace NLog.Mongo.Infrastructure
{
    using NUnit.Framework;
    using Moq;

    [TestFixture]
    public class CacheKeyFactoryTests
    {
        private MockRepository _mockFactory;

        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
        }

        private MongoCollectionResolver.CacheKeyFactory Create()
        {
            return new MongoCollectionResolver.CacheKeyFactory();
        }

        [Test]
        public void CreateTest()
        {
            var settings = _mockFactory.Create<IMongoConnectionSettings>();

            settings.Setup(x => x.ConnectionName).Returns("ConnectionName").Verifiable();
            settings.Setup(x => x.ConnectionString).Returns("ConnectionString").Verifiable();
            settings.Setup(x => x.CollectionName).Returns("CollectionName").Verifiable();

            var res = Create().Create(settings.Object);

            Assert.AreEqual("k|ConnectionName|ConnectionString|CollectionName", res);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}