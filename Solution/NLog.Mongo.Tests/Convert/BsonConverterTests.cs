namespace NLog.Mongo.Convert
{
    using NUnit.Framework;
    using MongoDB.Bson;
    using Moq;
    using Moq.Protected;
    using NLog.Layouts;
    using NLog.Mongo.Infrastructure;

    [TestFixture]
    public class BsonConverterTests
    {
        private LogEventInfo _lei;
        private Mock<IBsonStructConvertMethodFactory> _methodFactory;
        private MockRepository _mockFactory;
        private Mock<IBsonStructConverter> _structConverter;


        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _structConverter = _mockFactory.Create<IBsonStructConverter>();
            _methodFactory = _mockFactory.Create<IBsonStructConvertMethodFactory>();
            _lei = new LogEventInfo();
        }

        private BsonConverter Create()
        {
            return new BsonConverter(_structConverter.Object, _methodFactory.Object);
        }


        [Test]
        public void GetValueNullFieldTest()
        {
            var res = Create().GetValue(null, _lei);
            Assert.IsNull(res);
        }

        [Test]
        public void GetValueTest1()
        {
            var l = _mockFactory.Create<TestLayout>();
            var mf = new MongoField("Name", l.Object, "skjgsdkljghsdkl;rghsdr;gh");
            const string value = "1122332";
            var bv = new BsonBoolean(true);

            l.Protected().Setup("InitializeLayout").Verifiable();
            l.Protected().Setup<string>("GetFormattedMessage", _lei).Returns(value).Verifiable();
            _structConverter.Setup(x => x.BsonString(value)).Returns(bv).Verifiable();

            var res = Create().GetValue(mf, _lei);

            Assert.AreEqual(bv, res);
        }

        [Test]
        public void GetValueTest2()
        {
            var l = _mockFactory.Create<TestLayout>();
            const BsonType bsonType = BsonType.Boolean;
            var mf = new MongoField("Name", l.Object, bsonType.ToString());
            const string value = "1122332";
            BsonValue bv = new BsonBoolean(true);
            var invoked = false;
            BsonTryConvertMethod m = (string s, out BsonValue converted) =>
            {
                converted = null;
                invoked = true;
                return false;
            };
            l.Protected().Setup("InitializeLayout").Verifiable();
            l.Protected().Setup<string>("GetFormattedMessage", _lei).Returns(value).Verifiable();
            _methodFactory.Setup(x => x.Create(bsonType)).Returns(m).Verifiable();
            _structConverter.Setup(x => x.BsonString(value)).Returns(bv).Verifiable();

            var res = Create().GetValue(mf, _lei);

            Assert.AreEqual(bv, res);
            Assert.IsTrue(invoked);
        }

        [Test]
        public void GetValueTest3()
        {
            var l = _mockFactory.Create<TestLayout>();
            const BsonType bsonType = BsonType.Boolean;
            var mf = new MongoField("Name", l.Object, bsonType.ToString());
            const string value = "1122332";
            BsonValue bv = new BsonBoolean(true);
            var invoked = false;
            BsonTryConvertMethod m = (string s, out BsonValue converted) =>
            {
                converted = bv;
                invoked = true;
                return true;
            };

            l.Protected().Setup("InitializeLayout").Verifiable();
            l.Protected().Setup<string>("GetFormattedMessage", _lei).Returns(value).Verifiable();
            _methodFactory.Setup(x => x.Create(bsonType)).Returns(m).Verifiable();

            var res = Create().GetValue(mf, _lei);

            Assert.AreEqual(bv, res);
            Assert.IsTrue(invoked);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }

        public class TestLayout : Layout
        {
            public virtual string GetFormattedMessageP(LogEventInfo logEvent)
            {
                return "";
            }


            protected override string GetFormattedMessage(LogEventInfo logEvent)
            {
                return GetFormattedMessageP(logEvent);
            }

            public virtual void InitializeLayoutP()
            {
            }

            protected override void InitializeLayout()
            {
                InitializeLayoutP();
            }
        }
    }
}