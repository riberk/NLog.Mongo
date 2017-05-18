namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using NLog.Mongo.Convert;

    internal class BsonDocumentCreator : IBsonDocumentCreator
    {
        [NotNull] private readonly IBsonConverter _bsonConverter;
        [NotNull] private readonly IBsonDocumentValueAppender _bsonDocumentValueAppender;
        [NotNull] private readonly IBsonPropertiesFactory _bsonPropertiesFactory;
        [NotNull] private readonly IDefaultsFactory _defaultsFactory;

        public BsonDocumentCreator([NotNull] IBsonDocumentValueAppender bsonDocumentValueAppender,
                                   [NotNull] IBsonConverter bsonConverter,
                                   [NotNull] IDefaultsFactory defaultsFactory,
                                   [NotNull] IBsonPropertiesFactory bsonPropertiesFactory)
        {
            if (bsonDocumentValueAppender == null) throw new ArgumentNullException(nameof(bsonDocumentValueAppender));
            if (bsonConverter == null) throw new ArgumentNullException(nameof(bsonConverter));
            if (defaultsFactory == null) throw new ArgumentNullException(nameof(defaultsFactory));
            if (bsonPropertiesFactory == null) throw new ArgumentNullException(nameof(bsonPropertiesFactory));
            _bsonDocumentValueAppender = bsonDocumentValueAppender;
            _bsonConverter = bsonConverter;
            _defaultsFactory = defaultsFactory;
            _bsonPropertiesFactory = bsonPropertiesFactory;
        }

        public BsonDocument CreateDocument(LogEventInfo logEvent,
                                           IReadOnlyCollection<MongoField> fields,
                                           IReadOnlyCollection<MongoField> properties,
                                           bool includeDefaults)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            var document = new BsonDocument();
            if (includeDefaults || !fields.Any())
            {
                foreach (var keyValuePair in _defaultsFactory.Create(logEvent))
                {
                    _bsonDocumentValueAppender.Append(document, keyValuePair.Key, keyValuePair.Value);
                }
            }

            foreach (var field in fields)
            {
                var value = _bsonConverter.GetValue(field, logEvent);
                _bsonDocumentValueAppender.Append(document, field.Name, value);
            }
            var props = _bsonPropertiesFactory.Create(properties, logEvent);
            _bsonDocumentValueAppender.Append(document, "Properties", props);

            return document;
        }
    }
}