namespace NLog.Mongo.Convert
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using MongoDB.Bson;
    using Moq;
    using NLog.Mongo.Infrastructure;

    [TestFixture]
    public class BsonStructConvertMethodFactoryTests
    {
        [NotNull] private Mock<IBsonStructConverter> _converter;
        [NotNull] private Dictionary<BsonType, BsonTryConvertMethod> _dict;

        [NotNull] private MockRepository _mockFactory;


        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _converter = _mockFactory.Create<IBsonStructConverter>();
            _dict = new Dictionary<BsonType, BsonTryConvertMethod>()
            {
                {BsonType.Double, _converter.Object.TryDouble},
                {BsonType.String, _converter.Object.TryString},
                {BsonType.Boolean, _converter.Object.TryBoolean},
                {BsonType.DateTime, _converter.Object.TryDateTime},
                {BsonType.Int32, _converter.Object.TryInt32},
                {BsonType.Int64, _converter.Object.TryInt64},
            };
        }

        private BsonStructConvertMethodFactory Create()
        {
            return new BsonStructConvertMethodFactory(_converter.Object);
        }

        [Test]
        public void CreateValidTest()
        {
            var c = Create();
            foreach (var m in _dict)
            {
                Assert.AreEqual(m.Value, c.Create(m.Key));
            }
        }

        [Test]
        public void CreateInvalidTest()
        {
            var c = Create();
            foreach (var t in Enum.GetValues(typeof (BsonType)).Cast<BsonType>().Where(bt => !_dict.ContainsKey(bt)))
            {
                try
                {
                    c.Create(t);
                    Assert.Fail("Не выдано исключение на неизвестный тип поля");
                }
                catch (NotSupportedException)
                {
                }
            }
        }

        [Test]
        public void CreateUndefinedTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create().Create((BsonType) 99999));
        }


        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }
    }
}