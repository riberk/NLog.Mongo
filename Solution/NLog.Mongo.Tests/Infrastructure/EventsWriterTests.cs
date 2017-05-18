namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Moq;
    using NLog.Common;

    [TestFixture]
    public class EventsWriterTests
    {
        private Mock<IMongoCollection<BsonDocument>> _collection;
        private Mock<IMongoCollectionResolver> _collectionResolver;
        private Mock<IBsonDocumentCreator> _docCreator;
        private MockRepository _mockFactory;
        private Mock<IMongoTarget> _target;


        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _docCreator = _mockFactory.Create<IBsonDocumentCreator>();
            _collectionResolver = _mockFactory.Create<IMongoCollectionResolver>();
            _target = _mockFactory.Create<IMongoTarget>();
            _collection = _mockFactory.Create<IMongoCollection<BsonDocument>>();
        }

        private EventsWriter Create()
        {
            return new EventsWriter(_docCreator.Object, _collectionResolver.Object);
        }

        [Test]
        public void WriteNullArgument1Test() => Assert.Throws<ArgumentNullException>(() => Create().Write(null, _target.Object));

        [Test]
        public void WriteNullArgument2Test() => Assert.Throws<ArgumentNullException>(() => Create().Write(new AsyncLogEventInfo[0], null));

        [Test]
        public void WriteTest()
        {
            var e1 = new LogEventInfo();
            var e2 = new LogEventInfo();
            var events = new AsyncLogEventInfo[]
            {
                new AsyncLogEventInfo(e1, null),
                new AsyncLogEventInfo(e2, null)
            };
            var d1 = new BsonDocument();
            var d2 = new BsonDocument();
            var fields = new List<MongoField>();
            var props = new List<MongoField>();
            _target.Setup(x => x.Fields).Returns(fields).Verifiable();
            _target.Setup(x => x.Properties).Returns(props).Verifiable();
            _target.Setup(x => x.IncludeDefaults).Returns(true).Verifiable();

            _docCreator.Setup(x => x.CreateDocument(e1, fields, props, true)).Returns(d1).Verifiable();
            _docCreator.Setup(x => x.CreateDocument(e2, fields, props, true)).Returns(d2).Verifiable();

            _collectionResolver.Setup(x => x.GetCollection(_target.Object)).Returns(_collection.Object).Verifiable();
            _collection
                    .Setup(x => x.InsertManyAsync(It.IsAny<IEnumerable<BsonDocument>>(), null, default(CancellationToken)))
                    .Returns((IEnumerable<BsonDocument> docs, InsertManyOptions o, CancellationToken t) =>
                    {
                        Assert.AreEqual(2, docs.Count());
                        Assert.IsTrue(docs.Contains(d1));
                        Assert.IsTrue(docs.Contains(d2));
                        return Task.CompletedTask;
                    }).Verifiable();
            Create().Write(events, _target.Object);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}