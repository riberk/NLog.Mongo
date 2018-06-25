using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using Moq;
using NLog.Common;
using NLog.Mongo.Infrastructure;
using NLog.Mongo.Infrastructure.Indexes;

namespace NLog.Mongo
{


    [TestFixture]
    public class MongoTargetTests
    {
        private Mock<IConnectionStringRetriever> _connectionStringRetriever;
        private Mock<IEventsWriter> _eventsWriter;
        private Mock<MongoTarget.IInternalLogger> _internalLogger;
        private MockRepository _mockFactory;
        private Mock<IIndexesFactory> _indexesFacory;
        private Mock<IMongoCollectionResolver> _collectionResolver;


        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _connectionStringRetriever = _mockFactory.Create<IConnectionStringRetriever>();
            _eventsWriter = _mockFactory.Create<IEventsWriter>();
            _internalLogger = _mockFactory.Create<MongoTarget.IInternalLogger>();
            _indexesFacory = _mockFactory.Create<IIndexesFactory>();
            _collectionResolver = _mockFactory.Create<IMongoCollectionResolver>();
        }

        private TestTarget Create()
        {
            return new TestTarget(_connectionStringRetriever.Object, _eventsWriter.Object, _collectionResolver.Object, _indexesFacory.Object, _internalLogger.Object);
        }

        [Test]
        public void PropsTest()
        {
            var target = Create();
            const string connectionString = "connectionString";
            const string connectionName = "ConnectionName";
            const string collectionName = "CollectionName";
            const bool includeDefaults = true;
            const long cappedCollectionSize = 10;
            const long cappedCollectionMaxItems = 100;

            target.ConnectionString = connectionString;
            target.ConnectionName = connectionName;
            target.IncludeDefaults = includeDefaults;
            target.CollectionName = collectionName;
            target.CappedCollectionSize = cappedCollectionSize;
            target.CappedCollectionMaxItems = cappedCollectionMaxItems;


            Assert.IsNotNull(target.Properties);
            Assert.IsNotNull(target.Fields);
            Assert.AreEqual(0, target.Properties.Count);
            Assert.AreEqual(0, target.Fields.Count);
            Assert.AreEqual(connectionName, target.ConnectionName);
            Assert.AreEqual(connectionString, target.ConnectionString);
            Assert.AreEqual(collectionName, target.CollectionName);
            Assert.AreEqual(includeDefaults, target.IncludeDefaults);
            Assert.AreEqual(cappedCollectionSize, target.CappedCollectionSize);
            Assert.AreEqual(cappedCollectionMaxItems, target.CappedCollectionMaxItems);
        }

        [Test]
        public void WriteZeroEventsTest()
        {
            Create().WriteAsync(new AsyncLogEventInfo[0]);
        }

        [Test]
        public void WriteNullEventsTest()
        {
            Create().WriteAsync(null);
        }

        [Test]
        public void WriteTest()
        {
            var firstExecuted = false;
            var secondExecuted = false;

            void FirstContinuation(Exception exception)
            {
                Assert.IsNull(exception);
                firstExecuted = true;
            }

            void SecondContinuation(Exception exception)
            {
                Assert.IsNull(exception);
                secondExecuted = true;
            }

            var events = new[]
            {
                new AsyncLogEventInfo(new LogEventInfo(), FirstContinuation),
                new AsyncLogEventInfo(new LogEventInfo(), SecondContinuation),
            };
            var testTarget = Create();
            _eventsWriter.Setup(x => x.Write(events, testTarget)).Verifiable();

            testTarget.WriteAsync(events);

            Assert.IsTrue(firstExecuted);
            Assert.IsTrue(secondExecuted);
        }

        [Test]
        public void WriteWithExceptionTest()
        {
            var firstExecuted = false;
            var secondExecuted = false;
            var exception = new Exception("Message");

            void FirstContinuation(Exception e)
            {
                Assert.AreEqual(exception, e);
                firstExecuted = true;
            }

            void SecondContinuation(Exception e)
            {
                Assert.AreEqual(exception, e);
                secondExecuted = true;
            }

            var events = new[]
            {
                new AsyncLogEventInfo(new LogEventInfo(), FirstContinuation),
                new AsyncLogEventInfo(new LogEventInfo(), SecondContinuation),
            };
            var testTarget = Create();
            _eventsWriter.Setup(x => x.Write(events, testTarget)).Throws(exception).Verifiable();
            _internalLogger.Setup(x => x.Error(It.IsAny<string>(), exception)).Verifiable();

            testTarget.WriteAsync(events);

            Assert.IsTrue(firstExecuted);
            Assert.IsTrue(secondExecuted);
        }

        [Test]
        public void WriteWithUnsupportedExceptionTest()
        {
            var events = new []
            {
                new AsyncLogEventInfo()
            };
            var testTarget = Create();
            var exception = new OutOfMemoryException();
            _eventsWriter.Setup(x => x.Write(events, testTarget)).Throws(exception).Verifiable();
            try
            {
                testTarget.WriteAsync(events);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(exception, e);
            }
        }

        [Test]
        public void InitializeTargetWithConnectionStringTest()
        {
            var testTarget = Create();
            testTarget.ConnectionString = "str";
            var col = _mockFactory.Create<IMongoCollection<BsonDocument>>(MockBehavior.Strict);
            _collectionResolver.Setup(x => x.GetCollection(testTarget)).Returns(col.Object).Verifiable();

            _indexesFacory.Setup(x => x.Create(new CreateIndexesContext<BsonDocument>(testTarget.Indexes, col.Object)))
                          .Returns(Task.CompletedTask)
                          .Verifiable();
            testTarget.InitializeTargetImpl();

        }

        [Test]
        public void InitializeTargetWithNullConnectionNameTest()
        {
            var testTarget = Create();
            Assert.Throws<NLogConfigurationException>(() => testTarget.InitializeTargetImpl());
        }

        [Test]
        public void InitializeTargetTest()
        {
            const string connectionName = "name";
            const string connectionString = "string";
            var testTarget = Create();
            testTarget.ConnectionName = connectionName;
            _connectionStringRetriever.Setup(x => x.GetConnectionString(connectionName)).Returns(connectionString);

            Assert.IsNull(testTarget.ConnectionString);
            var col = _mockFactory.Create<IMongoCollection<BsonDocument>>(MockBehavior.Strict);
            _collectionResolver.Setup(x => x.GetCollection(testTarget)).Returns(col.Object).Verifiable();

            _indexesFacory.Setup(x => x.Create(new CreateIndexesContext<BsonDocument>(testTarget.Indexes, col.Object)))
                          .Returns(Task.CompletedTask)
                          .Verifiable();
            testTarget.InitializeTargetImpl();
            Assert.AreEqual(connectionString, testTarget.ConnectionString);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }

        public class TestTarget : MongoTarget
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="MongoTarget" /> class.
            /// </summary>
            public TestTarget([NotNull] IConnectionStringRetriever connectionStringRetriever,
                              [NotNull] IEventsWriter eventsWriter,
                              IMongoCollectionResolver collectionResolver,
                              IIndexesFactory factory,
                              [NotNull] IInternalLogger internalLogger) : base(connectionStringRetriever, eventsWriter, factory, collectionResolver, internalLogger)
            {
            }


            public void WriteAsync(IList<AsyncLogEventInfo> logEvents)
            {
                Write(logEvents);
            }

            public void InitializeTargetImpl()
            {
                InitializeTarget();
            }
        }
    }
}