namespace NLog.Mongo
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class InternalLoggerImplTests
    {
        [Test]
        public void ErrorTest()
        {
            new MongoTarget.InternalLoggerImpl().Error("123", new Exception());
        }
    }
}