namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using MongoDB.Bson;
    using Moq;
    using NLog.Mongo.Convert;

    [TestFixture]
    public class BsonPropertiesFactoryTests
    {
        private Mock<IBsonConverter> _iBsonConverter;
        private Mock<IBsonDocumentValueAppender> _iBsonDocumentValueAppender;
        private Mock<IBsonStructConverter> _iBsonStructConverter;
        private MockRepository _mockFactory;


        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _iBsonConverter = _mockFactory.Create<IBsonConverter>();
            _iBsonDocumentValueAppender = _mockFactory.Create<IBsonDocumentValueAppender>();
            _iBsonStructConverter = _mockFactory.Create<IBsonStructConverter>();
        }

        private BsonPropertiesFactory Create()
        {
            return new BsonPropertiesFactory(_iBsonConverter.Object, _iBsonDocumentValueAppender.Object, _iBsonStructConverter.Object);
        }

        [Test]
        public void CreatenullArgument1Test() => Assert.Throws<ArgumentNullException>(() => Create().Create(null, new LogEventInfo()));

        [Test]
        public void CreatenullArgument2Test() => Assert.Throws<ArgumentNullException>(() => Create().Create(new MongoField[0], null));

        [Test]
        public void CreateNullResultTest()
        {
            var e = new LogEventInfo();
            var res = Create().Create(Enumerable.Empty<MongoField>(), e);
            Assert.AreEqual(BsonNull.Value, res);
        }
        [Test]
        public void CreateTest()
        {
            var props = new[]
            {
                new MongoField("F1", new BsonConverterTests.TestLayout()),
                new MongoField("F2", new BsonConverterTests.TestLayout()),
            };
            var v1 = _mockFactory.Create<BsonValue>();
            var v2 = _mockFactory.Create<BsonValue>();

            var e = new LogEventInfo();

            _iBsonConverter.Setup(x => x.GetValue(props[0], It.IsAny<LogEventInfo>())).Returns((MongoField f, LogEventInfo ev) =>
            {
                Assert.AreEqual(e, ev);
                return v1.Object;
            }).Verifiable();

            _iBsonConverter.Setup(x => x.GetValue(props[1], It.IsAny<LogEventInfo>())).Returns((MongoField f, LogEventInfo ev) =>
            {
                Assert.AreEqual(e, ev);
                return v2.Object;
            }).Verifiable();

            _iBsonDocumentValueAppender
                    .Setup(x => x.Append(It.IsAny<BsonDocument>(), props[0].Name, It.IsAny<BsonValue>()))
                    .Callback((BsonDocument d, string s, BsonValue v) =>
                    {
                        Assert.AreEqual(v1.Object, v);
                        d.Add(new BsonElement(s, v));
                    }).Verifiable();

            _iBsonDocumentValueAppender
                    .Setup(x => x.Append(It.IsAny<BsonDocument>(), props[1].Name, It.IsAny<BsonValue>()))
                    .Callback((BsonDocument d, string s, BsonValue v) =>
                    {
                        Assert.AreEqual(v2.Object, v);
                        d.Add(new BsonElement(s, v));
                    }).Verifiable();

            const string ep1 = "ep1";
            const string ep2 = "ep2";
            e.Properties.Add(ep1, "ev1");
            e.Properties.Add(ep2, "ev2");
            var ev1 = _mockFactory.Create<BsonValue>();
            var ev2 = _mockFactory.Create<BsonValue>();

            _iBsonStructConverter.Setup(x => x.BsonString(e.Properties[ep1].ToString())).Returns(ev1.Object).Verifiable();
            _iBsonStructConverter.Setup(x => x.BsonString(e.Properties[ep2].ToString())).Returns(ev2.Object).Verifiable();

            _iBsonDocumentValueAppender
                    .Setup(x => x.Append(It.IsAny<BsonDocument>(), ep1, It.IsAny<BsonValue>()))
                    .Callback((BsonDocument d, string s, BsonValue v) =>
                    {
                        Assert.AreEqual(ev1.Object, v);
                        d.Add(new BsonElement(s, v));
                    }).Verifiable();
            _iBsonDocumentValueAppender
                    .Setup(x => x.Append(It.IsAny<BsonDocument>(), ep2, It.IsAny<BsonValue>()))
                    .Callback((BsonDocument d, string s, BsonValue v) =>
                    {
                        Assert.AreEqual(ev2.Object, v);
                        d.Add(new BsonElement(s, v));
                    }).Verifiable();


            var res = Create().Create(props, e) as BsonDocument;

            Assert.AreEqual(4, res.ElementCount);
            var resDict = res.Elements.ToDictionary(x => x.Name, x => x.Value);

            Assert.AreEqual(resDict[props[0].Name], v1.Object);
            Assert.AreEqual(resDict[props[1].Name], v2.Object);
            Assert.AreEqual(resDict[ep1], ev1.Object);
            Assert.AreEqual(resDict[ep2], ev2.Object);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}