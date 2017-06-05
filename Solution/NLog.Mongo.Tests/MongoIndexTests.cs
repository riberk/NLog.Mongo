namespace NLog.Mongo
{
    using System;
    using System.Reflection;
    using NLog.Mongo.Infrastructure.Indexes;
    using NUnit.Framework;

    [TestFixture]
    public class MongoIndexTests
    {
        [Test]
        public void PropsTest()
        {
            var idx = new MongoIndex();
            Assert.IsNotNull(idx.Fields);
            Assert.AreEqual(idx.IndexFields, idx.Fields);

            var property = typeof(MongoIndex).GetProperty(nameof(MongoIndex.CreationBehaviour));
            property.SetValue(idx, "Replace");
            Assert.AreEqual("Replace", idx.CreationBehaviour);
            Assert.AreEqual(CreationBehaviour.Replace, idx.IndexCreationBehaviour);

            var ex = Assert.Throws<TargetInvocationException>(() => property.SetValue(idx, "djghsdjkgsdjkgh"));
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOf<FormatException>(ex.InnerException);
        }
    }
}