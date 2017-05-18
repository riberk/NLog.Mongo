namespace NLog.Mongo.Infrastructure
{
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MongoDB.Bson;

    public interface IBsonPropertiesFactory
    {
        [NotNull]
        BsonValue Create([NotNull] IEnumerable<MongoField> props, [NotNull] LogEventInfo logEvent);
    }
}