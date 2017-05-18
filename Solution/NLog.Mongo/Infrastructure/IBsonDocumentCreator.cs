namespace NLog.Mongo.Infrastructure
{
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MongoDB.Bson;

    public interface IBsonDocumentCreator
    {
        BsonDocument CreateDocument([NotNull] LogEventInfo logEvent,
                                    [NotNull] IReadOnlyCollection<MongoField> fields,
                                    [NotNull] IReadOnlyCollection<MongoField> properties,
                                    bool includeDefaults);
    }
}