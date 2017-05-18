﻿namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using NLog.Common;
    using NLog.Mongo.Internal;

    internal class EventsWriter : IEventsWriter
    {
        [NotNull] private readonly IBsonDocumentCreator _bsonDocumentCreator;

        [NotNull] private readonly IMongoCollectionResolver _mongoCollectionResolver;

        public EventsWriter([NotNull] IBsonDocumentCreator bsonDocumentCreator, [NotNull] IMongoCollectionResolver mongoCollectionResolver)
        {
            if (bsonDocumentCreator == null) throw new ArgumentNullException(nameof(bsonDocumentCreator));
            if (mongoCollectionResolver == null) throw new ArgumentNullException(nameof(mongoCollectionResolver));
            _bsonDocumentCreator = bsonDocumentCreator;
            _mongoCollectionResolver = mongoCollectionResolver;
        }

        public void Write(AsyncLogEventInfo[] logEvents, IMongoTarget target)
        {
            if (logEvents == null) throw new ArgumentNullException(nameof(logEvents));
            if (target == null) throw new ArgumentNullException(nameof(target));
            var documents = logEvents.Select(e => _bsonDocumentCreator.CreateDocument(e.LogEvent, target.Fields, target.Properties, target.IncludeDefaults));
            var collection = _mongoCollectionResolver.GetCollection(target);
            AsyncHelper.RunSync(() => collection.InsertManyAsync(documents));
        }
    }
}