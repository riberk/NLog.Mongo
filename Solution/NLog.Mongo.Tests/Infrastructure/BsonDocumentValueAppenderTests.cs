namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using MongoDB.Bson;
    using Moq;

    [TestFixture]
    public class BsonDocumentValueAppenderTests
    {
        private MockRepository _mockFactory;

        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
        }

        private BsonDocumentValueAppender Create()
        {
            return new BsonDocumentValueAppender();
        }

        [Test]
        public void AppendNullArgument1Test() 
            => Assert.Throws<ArgumentNullException>(() => Create().Append(null, "123", new BsonBoolean(true)));

        [Test]
        public void AppendNullArgument2Test() 
            => Assert.Throws<ArgumentNullException>(() => Create().Append(new BsonDocument(), null, new BsonBoolean(true)));

        [Test]
        public void AppendNullTest()
        {
            var bd = new BsonDocument();
            Assert.AreEqual(0, bd.ElementCount);
            Create().Append(bd, "123", null);
            Assert.AreEqual(0, bd.ElementCount);
        }

        [Test]
        public void AppendBsonNullTest()
        {
            var bd = new BsonDocument();
            Assert.AreEqual(0, bd.ElementCount);
            Create().Append(bd, "123", BsonNull.Value);
            Assert.AreEqual(0, bd.ElementCount);
        }

        [Test]
        public void AppendTest()
        {
            var bd = new BsonDocument();
            var bv = new BsonBoolean(true);
            Assert.AreEqual(0, bd.ElementCount);
            Create().Append(bd, "123", bv);
            Assert.AreEqual(1, bd.ElementCount);
            Assert.AreEqual(bv, bd.Elements.First().Value);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}