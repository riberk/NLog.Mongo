namespace NLog.Mongo
{
    using NUnit.Framework;
    using Moq;
    using NLog.Layouts;

    [TestFixture]
    public class MongoFieldTests
    {
        private Mock<Layout> _layout;
        private MockRepository _mockFactory;

        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _layout = _mockFactory.Create<Layout>();
        }

        [Test]
        public void MongoFieldTest()
        {
            var mf = new MongoField();
            Assert.IsNull(mf.Name);
            // ReSharper disable HeuristicUnreachableCode
            Assert.IsNull(mf.Layout);
            Assert.AreEqual("String", mf.BsonType);
            // ReSharper restore HeuristicUnreachableCode
        }

        [Test]
        public void MongoFieldTest1()
        {
            const string name = "Name";

            var mf = new MongoField(name, _layout.Object);

            Assert.AreEqual(name, mf.Name);
            Assert.AreEqual(_layout.Object, mf.Layout);
            Assert.AreEqual("String", mf.BsonType);
        }

        [Test]
        public void MongoFieldTest2()
        {
            const string name = "Name";
            const string bsonType = "Wtf";

            var mf = new MongoField(name, _layout.Object, bsonType);

            Assert.AreEqual(name, mf.Name);
            Assert.AreEqual(_layout.Object, mf.Layout);
            Assert.AreEqual(bsonType, mf.BsonType);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}