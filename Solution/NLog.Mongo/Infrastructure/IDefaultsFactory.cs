namespace NLog.Mongo.Infrastructure
{
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MongoDB.Bson;

    public interface IDefaultsFactory
    {
        [NotNull]
        IEnumerable<KeyValuePair<string, BsonValue>> Create([NotNull] LogEventInfo logEvent);
    }
}