namespace NLog.Mongo
{
    using JetBrains.Annotations;
    using NLog.Config;
    using NLog.Layouts;

    public class MongoIndex : IMongoIndexOptions
    {
        /// <summary>
        ///     Gets or sets a value indicating whether to create the index in the background.
        /// </summary>
        public bool? Background { get; [UsedImplicitly] private set; }

        /// <summary>
        ///     Gets or sets the precision, in bits, used with geohash indexes.
        /// </summary>
        public int? Bits { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the size of a geohash bucket.</summary>
        public double? BucketSize { get; [UsedImplicitly] private set; }

        //TODO Collation
        //!!public Collation Collation { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the default language.</summary>
        public string DefaultLanguage { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the language override.</summary>
        public string LanguageOverride { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the max value for 2d indexes.</summary>
        public double? Max { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the min value for 2d indexes.</summary>
        public double? Min { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the index name.</summary>
        [NotNull]
        [RequiredParameter]
        public string Name { get; [UsedImplicitly] private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the index is a sparse index.
        /// </summary>
        public bool? Sparse { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the index version for 2dsphere indexes.</summary>
        public int? SphereIndexVersion { get; [UsedImplicitly] private set; }

        //TODO Storage engine
        //public BsonDocument StorageEngine { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the index version for text indexes.</summary>
        public int? TextIndexVersion { get; [UsedImplicitly] private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the index is a unique index.
        /// </summary>
        public bool? Unique { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the version of the index.</summary>
        public int? Version { get; [UsedImplicitly] private set; }

        //TODO Weights
        //public BsonDocument Weights { get; [UsedImplicitly] private set; }

        /// <summary>
        ///     Gets or sets the layout used to generate the value for the field.
        /// </summary>
        /// <value>
        ///     The layout used to generate the value for the field.
        /// </value>
        [NotNull]
        [RequiredParameter]
        public Layout Layout { get; [UsedImplicitly] private set; }
    }
}