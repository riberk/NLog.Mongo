namespace NLog.Mongo.Convert
{
    using System;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using MongoDB.Bson;
    using Moq;
    using NLog.Mongo.Infrastructure;

    [TestFixture]
    public class BsonStructConverterTests
    {
        [NotNull] private MockRepository _mockFactory;

        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
        }

        [NotNull]
        private BsonStructConverter Create()
        {
            return new BsonStructConverter();
        }

        [Test]
        public void TryBooleanTest()
        {
            Check<BsonBoolean, bool>(Create().TryBoolean, "true", true, v => v.AsBoolean, true);
        }

        [Test]
        public void TryDateTimeTest()
        {
            Check<BsonDateTime, DateTime>(Create().TryDateTime, "2015-11-10Z", true, v => v.ToUniversalTime(),
                                          new DateTime(2015, 11, 10, 0, 0, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TryDoubleTest()
        {
            Check<BsonDouble, double>(Create().TryDouble, "1,225", true, v => v.AsDouble, 1.225);
        }

        [Test]
        public void TryInt32Test()
        {
            Check<BsonInt32, int>(Create().TryInt32, "15", true, v => v.AsInt32, 15);
        }

        [Test]
        public void TryInt64Test()
        {
            Check<BsonInt64, long>(Create().TryInt64, "15000000", true, v => v.AsInt64, 15000000);
        }

        [Test]
        public void TryStringTest()
        {
            Check<BsonString, string>(Create().TryString, "qqqq", true, v => v.AsString, "qqqq");
        }

        [Test]
        public void TryNullTest()
        {
            BsonValue v;
            var res = Create().TryDouble(null, out v);
            Assert.IsFalse(res);
            Assert.IsNull(v);
        }

        [Test]
        public void BsonStringTest()
        {
            var res = Create().BsonString("123");
            Assert.IsTrue(res is BsonString);
            Assert.AreEqual(res.AsString, "123");
        }

        [Test]
        public void BsonStringNullTest()
        {
            var res = Create().BsonString(null);
            Assert.IsTrue(res is BsonNull);
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }

        private static void Check<T, TV>([NotNull] BsonTryConvertMethod method, string value, bool r, [NotNull] Func<BsonValue, object> valueReader, TV exp)
                where T : BsonValue
        {
            BsonValue v;
            var res = method(value, out v);
            Assert.AreEqual(r, res);
            Assert.IsTrue(v is T);
            Assert.AreEqual(exp, valueReader(v));
        }
    }
}