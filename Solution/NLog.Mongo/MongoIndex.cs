namespace NLog.Mongo
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Mongo.Infrastructure.Indexes;


    public class MongoIndex : IMongoIndexOptions
    {
        [NotNull]
        [ItemNotNull]
        [ArrayParameter(typeof(MongoIndexField), "field")]
        public IReadOnlyList<MongoIndexField> Fields { get; } = new List<MongoIndexField>();

        /// <summary>
        ///     <seealso cref="CreationBehaviour" />
        /// </summary>
        [RequiredParameter]
        public string CreationBehaviour
        {
            get { return IndexCreationBehaviour.ToString(); }
            [UsedImplicitly]
            private set
            {
                CreationBehaviour t;
                if (!Enum.TryParse(value, true, out t))
                {
                    throw new FormatException("Coud not parse index creation behaviour");
                }
                IndexCreationBehaviour = t;
            }
        }

        /// <summary>Gets or sets the index name.</summary>
        [RequiredParameter]
        public string Name { get; [UsedImplicitly] private set; }

        public IReadOnlyList<IMongoIndexField> IndexFields => Fields;

        public CreationBehaviour IndexCreationBehaviour { get; private set; }

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
        //public Collation Collation { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the default language.</summary>
        public string DefaultLanguage { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the language override.</summary>
        public string LanguageOverride { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the max value for 2d indexes.</summary>
        public double? Max { get; [UsedImplicitly] private set; }

        /// <summary>Gets or sets the min value for 2d indexes.</summary>
        public double? Min { get; [UsedImplicitly] private set; }

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
    }
}