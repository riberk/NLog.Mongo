namespace NLog.Mongo.Infrastructure
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class ConnectionStringRetrieverTests
    {
#if !CORE
        [Test]
        public void GetConnectionStringNullTest() =>
            Assert.Throws<NLogConfigurationException>(() => new ConnectionStringRetriever().GetConnectionString("dfkjbnxdfklbjbj"));
        [Test]
        public void GetConnectionStringNullArgumentTest() 
            => Assert.Throws<ArgumentNullException>(() => new ConnectionStringRetriever().GetConnectionString(null));
        [Test]
        public void GetConnectionStringTest()
        {
            Assert.AreEqual("Connection", new ConnectionStringRetriever().GetConnectionString("Mongo"));
        }
#endif
    }
}