namespace NLog.Mongo.Infrastructure.Indexes
{
    using System;
    using System.Linq;
    using MongoDB.Bson;
    using NUnit.Framework;

    [TestFixture]
    public class IndexKeyFactoryTests
    {
        [Test]
        public void CreateForAllTest()
        {
            const string fieldName = "123dad";
            var indexKeyFactory = new IndexKeyFactory();
            foreach (var fieldIndexType in typeof(FieldIndexType).GetEnumValues().Cast<FieldIndexType>())
            {
                indexKeyFactory.Create<BsonDocument>(fieldIndexType, fieldName);
            }
        }

        [Test]
        public void CreateOutOfRangeTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new IndexKeyFactory().Create<BsonDocument>((FieldIndexType) (-210), "1"));
        }

        [Test]
        public void CreateNullFieldNameTest()
        {
            Assert.Throws<ArgumentNullException>(() => new IndexKeyFactory().Create<BsonDocument>(FieldIndexType.Ascending, null));
        }
    }
}