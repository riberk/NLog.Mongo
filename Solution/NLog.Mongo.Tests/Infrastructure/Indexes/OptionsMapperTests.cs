namespace NLog.Mongo.Infrastructure.Indexes
{
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class OptionsMapperTests
    {
        [Test]
        public void MapTest()
        {
            var opts = new Mock<IMongoIndexOptions>(MockBehavior.Strict);
            const string expName = "expName";
            const bool expBackground = true;
            const int expBits = 155;
            const string expDefaultLanguage = "expDefaultLanguage";
            const string expLanguageOverride = "expLanguageOverride";
            const double expMax = 1.1;
            const double expBucketSize = 3.1;
            const double expMin = 0.1;
            const bool expSparse = false;
            const int expSphereIndexVersion = 10;
            const int expTextIndexVersion = 11;
            const bool expUnique = true;
            const int expVersion = 12;

            opts.Setup(x => x.Name).Returns(expName).Verifiable();
            opts.Setup(x => x.Background).Returns(expBackground).Verifiable();
            opts.Setup(x => x.Bits).Returns(expBits).Verifiable();
            opts.Setup(x => x.BucketSize).Returns(expBucketSize).Verifiable();
            opts.Setup(x => x.DefaultLanguage).Returns(expDefaultLanguage).Verifiable();
            opts.Setup(x => x.LanguageOverride).Returns(expLanguageOverride).Verifiable();
            opts.Setup(x => x.Max).Returns(expMax).Verifiable();
            opts.Setup(x => x.Min).Returns(expMin).Verifiable();
            opts.Setup(x => x.Sparse).Returns(expSparse).Verifiable();
            opts.Setup(x => x.SphereIndexVersion).Returns(expSphereIndexVersion).Verifiable();
            opts.Setup(x => x.TextIndexVersion).Returns(expTextIndexVersion).Verifiable();
            opts.Setup(x => x.Unique).Returns(expUnique).Verifiable();
            opts.Setup(x => x.Version).Returns(expVersion).Verifiable();


            var actual = new OptionsMapper().Map(opts.Object);

            Assert.AreEqual(expName, actual.Name);
            Assert.AreEqual(expBackground, actual.Background);
            Assert.AreEqual(expBits, actual.Bits);
            Assert.AreEqual(expBucketSize, actual.BucketSize);
            Assert.AreEqual(expDefaultLanguage, actual.DefaultLanguage);
            Assert.AreEqual(expLanguageOverride, actual.LanguageOverride);
            Assert.AreEqual(expMax, actual.Max);
            Assert.AreEqual(expMin, actual.Min);
            Assert.AreEqual(expSparse, actual.Sparse);
            Assert.AreEqual(expSphereIndexVersion, actual.SphereIndexVersion);
            Assert.AreEqual(expTextIndexVersion, actual.TextIndexVersion);
            Assert.AreEqual(expUnique, actual.Unique);
            Assert.AreEqual(expVersion, actual.Version);

            opts.VerifyAll();
        }
    }
}