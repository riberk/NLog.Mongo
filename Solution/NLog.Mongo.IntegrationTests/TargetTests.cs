namespace NLog.Mongo.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using NUnit.Framework;

    [TestFixture]
    public class TargetTests
    {
        private IMongoCollection<LogEntry> _collection;
        private const string LoggerName = "LoggerName";
        private const string MessageText = "Message text";
        private const string ExceptionMessage = "Exception message";
        private const int ErrorCode = 123456;

        [SetUp]
        public void SetUp()
        {
            var mongoUrl = new MongoUrl("mongodb://localhost/NlogMongoIntegrationTests");
            var client = new MongoClient(mongoUrl);
            var db = client.GetDatabase(mongoUrl.DatabaseName);
            db.DropCollection("Logs");
            db.CreateCollection("Logs");
            _collection = db.GetCollection<LogEntry>("Logs");
        }

        
        private async Task<LogEntry> AssertCommon(string level)
        {
            using (var cursor = await _collection.FindAsync(x => true))
            {
                Assert.IsTrue(await cursor.MoveNextAsync());
                var entries = cursor.Current.ToList();
                Assert.IsFalse(await cursor.MoveNextAsync());
                Assert.AreEqual(1, entries.Count);
                var entry = entries.Single();
                Assert.AreEqual(level, entry.Level);
                Assert.AreEqual(LoggerName, entry.Logger);
                Assert.AreEqual(MessageText, entry.Message);
                Assert.AreEqual("Field text", entry.AnyText);
                Assert.IsNotNull(entry.Properties);
                Assert.IsNotNull(entry.Properties["Prop"]);
                Assert.AreEqual("Prop text", entry.Properties["Prop"].AsString);
                return entry;
            }
        }

        [Test]
        public async Task WithoutException_WithoutEventProperties()
        {
            var logger = LogManager.GetLogger(LoggerName);
            logger.Info(MessageText);
            LogManager.Flush();
            var entry = await AssertCommon("Info");
            Assert.IsNull(entry.Exception);
        }

        [Test]
        public async Task WithoutException_WithEventProperties()
        {
            var logger = LogManager.GetLogger(LoggerName);
            var logEventInfo = new LogEventInfo(LogLevel.Info, LoggerName, MessageText);
            const string eventPropName = "PropFromEvent";
            const string eventPropValue = "123asefsmafl";

            logEventInfo.Properties[eventPropName] = eventPropValue;
            logger.Log(logEventInfo);
            LogManager.Flush();

            var entry = await AssertCommon("Info");
            Assert.IsNull(entry.Exception);
            Assert.IsNotNull(entry.Properties[eventPropName]);
            Assert.AreEqual(eventPropValue, entry.Properties[eventPropName].AsString);
        }

        [Test]
        public async Task WitException_WithoutEventProperties()
        {
            var logger = LogManager.GetLogger(LoggerName);
            var logEventInfo = new LogEventInfo(LogLevel.Info, LoggerName, MessageText);
            try
            {
                ThrowException();
                Assert.Fail();
            }
            catch (Exception e)
            {
                logger.Error(e, MessageText);
                LogManager.Flush();
            }

            var entry = await AssertCommon("Error");
            Assert.IsNotNull(entry.Exception);
            Assert.AreEqual(ExceptionMessage, entry.Exception.Message);
            Assert.AreEqual(ErrorCode, entry.Exception.ErrorCode);
            Assert.IsNotNull(entry.Exception.Stack);
            Assert.IsNotNull(typeof(ExternalException).ToString(), entry.Exception.Type);
        }

        private static void ThrowException()
        {
            ThrowExceptionTrace();
        }

        private static void ThrowExceptionTrace()
        {
            throw new ExternalException(ExceptionMessage, ErrorCode);
        }

    }

    public class LogEntry
    {
        public ObjectId Id { get; set; }

        public DateTime? Date { get; set; }

        public string Level { get; set; }

        public string Logger { get; set; }

        public string Message { get; set; }

        public BsonDocument Properties { get; set; }

        public LogException Exception { get; set; }

        public string AnyText { get; set; }
    }

    public class LogException
    {
        public string Message { get; set; }

        public string BaseMessage { get; set; }

        public string Text { get; set; }

        public string Type { get; set; }

        public string Stack { get; set; }

        public int? ErrorCode { get; set; }

        public string Source { get; set; }

        public string MethodName { get; set; }

        public string ModuleName { get; set; }

        public string ModuleVersion { get; set; }
    }
}