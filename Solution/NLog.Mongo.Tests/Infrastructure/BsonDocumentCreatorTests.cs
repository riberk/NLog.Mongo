namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using MongoDB.Bson;
    using Moq;
    using NLog.Mongo.Convert;

    [TestFixture]
    public class BsonDocumentCreatorTests
    {
        private Mock<IBsonConverter> _iBsonConverter;
        private Mock<IBsonDocumentValueAppender> _iBsonDocumentValueAppender;
        private Mock<IBsonPropertiesFactory> _iBsonPropertiesFactory;
        private Mock<IDefaultsFactory> _iDefaultsFactory;
        private MockRepository _mockFactory;


        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _iBsonDocumentValueAppender = _mockFactory.Create<IBsonDocumentValueAppender>();
            _iBsonConverter = _mockFactory.Create<IBsonConverter>();
            _iDefaultsFactory = _mockFactory.Create<IDefaultsFactory>();
            _iBsonPropertiesFactory = _mockFactory.Create<IBsonPropertiesFactory>();
        }

        private BsonDocumentCreator Create()
        {
            return new BsonDocumentCreator(_iBsonDocumentValueAppender.Object, _iBsonConverter.Object, _iDefaultsFactory.Object, _iBsonPropertiesFactory.Object);
        }

        [Test]
        public void CreateNullArgument1Test() 
            => Assert.Throws<ArgumentNullException>(() => Create().CreateDocument(null, new ArraySegment<MongoField>(), new ArraySegment<MongoField>(), true));

        [Test]
        public void CreateNullArgument2Test() 
            => Assert.Throws<ArgumentNullException>(() => Create().CreateDocument(new LogEventInfo(), null, new ArraySegment<MongoField>(), true));

        [Test]
        public void CreateNullArgument3Test() 
            => Assert.Throws<ArgumentNullException>(() => Create().CreateDocument(new LogEventInfo(), new ArraySegment<MongoField>(), null, true));

        [Test]
        public void CreateTest()
        {
            var fields = new List<MongoField>
            {
                new MongoField("f1", new BsonConverterTests.TestLayout()),
                new MongoField("f2", new BsonConverterTests.TestLayout()),
            };
            var props = new List<MongoField>()
            {
                new MongoField("p1", new BsonConverterTests.TestLayout()),
                new MongoField("p2", new BsonConverterTests.TestLayout()),
            };

            var e = new LogEventInfo();

            var d1v = _mockFactory.Create<BsonValue>();
            var d2v = _mockFactory.Create<BsonValue>();
            var values = new Dictionary<string, BsonValue>
            {
                {"d1", d1v.Object},
                {"d2", d2v.Object},
            };

            _iDefaultsFactory.Setup(x => x.Create(e)).Returns(values).Verifiable();

            foreach (var value in values)
            {
                _iBsonDocumentValueAppender
                        .Setup(x => x.Append(It.IsAny<BsonDocument>(), value.Key, It.IsAny<BsonValue>()))
                        .Callback((BsonDocument d, string s, BsonValue v) =>
                        {
                            Assert.AreEqual(value.Value, v);
                            d.Add(new BsonElement(s, value.Value));
                        }).Verifiable();
            }

            foreach (var mongoField in fields)
            {
                var fv = _mockFactory.Create<BsonValue>();
                _iBsonConverter.Setup(x => x.GetValue(mongoField, e)).Returns(fv.Object).Verifiable();
                _iBsonDocumentValueAppender
                        .Setup(x => x.Append(It.IsAny<BsonDocument>(), mongoField.Name, It.IsAny<BsonValue>()))
                        .Callback((BsonDocument d, string s, BsonValue v) =>
                        {
                            Assert.AreEqual(fv.Object, v);
                            d.Add(new BsonElement(s, v));
                        }).Verifiable();
            }

            var propsValue = _mockFactory.Create<BsonValue>();
            _iBsonPropertiesFactory.Setup(x => x.Create(props, e)).Returns(propsValue.Object).Verifiable();
            _iBsonDocumentValueAppender
                    .Setup(x => x.Append(It.IsAny<BsonDocument>(), "Properties", It.IsAny<BsonValue>()))
                    .Callback((BsonDocument d, string s, BsonValue v) =>
                    {
                        Assert.AreEqual(propsValue.Object, v);
                        d.Add(new BsonElement(s, v));
                    }).Verifiable();


            var result = Create().CreateDocument(e, fields, props, true);

            Assert.AreEqual(5, result.ElementCount);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}