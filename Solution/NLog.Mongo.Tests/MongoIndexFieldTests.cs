namespace NLog.Mongo
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class MongoIndexFieldTests
    {
        [Test]
        public void ATest()
        {
            var f = new MongoIndexField();
            var property = typeof(MongoIndexField).GetProperty(nameof(MongoIndexField.IndexType));
            property.SetValue(f, "Descending");
            Assert.AreEqual("Descending", f.IndexType);
            Assert.AreEqual(FieldIndexType.Descending, f.Type);

            var ex = Assert.Throws<TargetInvocationException>(() => property.SetValue(f, "djghsdjkgsdjkgh"));
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOf<FormatException>(ex.InnerException);
        }
    }
}