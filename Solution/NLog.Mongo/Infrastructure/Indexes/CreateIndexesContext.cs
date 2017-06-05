namespace NLog.Mongo.Infrastructure.Indexes
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public struct CreateIndexesContext<T>
    {
        public CreateIndexesContext([NotNull] IReadOnlyCollection<IMongoIndexOptions> indexes,
                                    [NotNull] IMongoCollection<T> collection)
        {
            Indexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        [NotNull]
        public IReadOnlyCollection<IMongoIndexOptions> Indexes { get; }

        [NotNull]
        public IMongoCollection<T> Collection { get; }
    }
}