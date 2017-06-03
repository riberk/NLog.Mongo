namespace NLog.Mongo
{
    using NLog.Layouts;
    using NLog.Mongo.Infrastructure.Indexes;

    public interface IMongoIndexOptions
    {
        bool? Background { get; }
        int? Bits { get; }
        double? BucketSize { get; }
        string DefaultLanguage { get; }
        string LanguageOverride { get; }
        Layout Layout { get; }
        double? Max { get; }
        double? Min { get; }
        string Name { get; }
        bool? Sparse { get; }
        int? SphereIndexVersion { get; }
        int? TextIndexVersion { get; }
        bool? Unique { get; }
        int? Version { get; }
        CreationBehaviour CreationBehaviour { get; }
    }
}