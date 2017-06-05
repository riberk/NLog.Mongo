namespace NLog.Mongo
{
    using System.Linq;
    using NUnit.Framework;


    [TestFixture]
    public class NullArgumentTest
    {
        [Test]
        public void AllNullArgumentThrow()
        {
            var res = ArgumentsVerifier.Builder(typeof(MongoTarget))
                                       .ToVerifier()
                                       .CheckAllCtorsWithoutParametersCreateObject()
                                       .CheckNullArgumentsOnConstructors()
                                       .Errors;
            if (res.Any())
            {
                Assert.Fail(string.Join("\r\n\r\n", res));
            }
        }
    }
}