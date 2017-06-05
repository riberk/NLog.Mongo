namespace NLog.Mongo
{
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using NLog.Layouts;
    using NLog.Mongo.Infrastructure.Indexes;

    public interface IMongoIndexOptions
    {
        [NotNull]
        IReadOnlyList<IMongoIndexField> IndexFields { get; }
        CreationBehaviour IndexCreationBehaviour { get; }
        bool? Background { get; }
        int? Bits { get; }
        double? BucketSize { get; }
        string DefaultLanguage { get; }
        string LanguageOverride { get; }
        double? Max { get; }
        double? Min { get; }
        string Name { get; }
        bool? Sparse { get; }
        int? SphereIndexVersion { get; }
        int? TextIndexVersion { get; }
        bool? Unique { get; }
        int? Version { get; }
    }
}