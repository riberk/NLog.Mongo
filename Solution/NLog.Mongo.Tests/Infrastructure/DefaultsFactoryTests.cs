namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Globalization;
    using System.Linq;
    using NUnit.Framework;
    using MongoDB.Bson;
    using Moq;
    using NLog.Mongo.Convert;

    [TestFixture]
    public class DefaultsFactoryTests
    {
        private Mock<IBsonExceptionFactory> _exceptionFactory;
        private MockRepository _mockFactory;
        private Mock<IBsonStructConverter> _structConverter;


        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _structConverter = _mockFactory.Create<IBsonStructConverter>();
            _exceptionFactory = _mockFactory.Create<IBsonExceptionFactory>();
        }

        private DefaultsFactory Create()
        {
            return new DefaultsFactory(_structConverter.Object, _exceptionFactory.Object);
        }

        [Test]
        public void CreateTest()
        {
            var logLevel = LogLevel.Error;
            const string loggerName = "LoggerName";
            const string message = "Message";
            var exception = new Exception();

            var bLevel = _mockFactory.Create<BsonValue>();
            var bLogger = _mockFactory.Create<BsonValue>();
            var bMessage = _mockFactory.Create<BsonValue>();
            var bException = _mockFactory.Create<BsonValue>();

            _structConverter.Setup(x => x.BsonString(logLevel.Name)).Returns(bLevel.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(loggerName)).Returns(bLogger.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(message)).Returns(bMessage.Object).Verifiable();
            _exceptionFactory.Setup(x => x.Create(exception)).Returns(bException.Object).Verifiable();
            var logEventInfo = new LogEventInfo(logLevel, loggerName, CultureInfo.InvariantCulture, message, null, exception);

            var actual = Create().Create(logEventInfo).ToDictionary(x => x.Key, x => x.Value);

            Assert.AreEqual(5, actual.Count);
            Assert.AreEqual(bLevel.Object, actual["Level"]);
            Assert.AreEqual(bLogger.Object, actual["Logger"]);
            Assert.AreEqual(bMessage.Object, actual["Message"]);
            Assert.AreEqual(bException.Object, actual["Exception"]);
            Assert.AreEqual(new BsonDateTime(logEventInfo.TimeStamp).ToUniversalTime(), actual["Date"].ToUniversalTime());
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}