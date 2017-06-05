namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using NLog.Mongo.Convert;

    internal class BsonPropertiesFactory : IBsonPropertiesFactory
    {
        [NotNull] private readonly IBsonConverter _bsonConverter;
        [NotNull] private readonly IBsonDocumentValueAppender _bsonDocumentValueAppender;
        [NotNull] private readonly IBsonStructConverter _bsonStructConverter;

        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="T:System.Object" />.
        /// </summary>
        public BsonPropertiesFactory([NotNull] IBsonConverter bsonConverter,
                                     [NotNull] IBsonDocumentValueAppender bsonDocumentValueAppender,
                                     [NotNull] IBsonStructConverter bsonStructConverter)
        {
            _bsonConverter = bsonConverter ?? throw new ArgumentNullException(nameof(bsonConverter));
            _bsonDocumentValueAppender = bsonDocumentValueAppender ?? throw new ArgumentNullException(nameof(bsonDocumentValueAppender));
            _bsonStructConverter = bsonStructConverter ?? throw new ArgumentNullException(nameof(bsonStructConverter));
        }

        public BsonValue Create(IEnumerable<MongoField> props, LogEventInfo logEvent)
        {
            if (props == null) throw new ArgumentNullException(nameof(props));
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var propertiesDocument = new BsonDocument();
            foreach (var prop in props)
            {
                _bsonDocumentValueAppender.Append(propertiesDocument, prop.Name, _bsonConverter.GetValue(prop, logEvent));
            }

            var properties = logEvent.Properties ?? Enumerable.Empty<KeyValuePair<object, object>>();
            foreach (var property in properties.Where(property => property.Key != null && property.Value != null))
            {
                _bsonDocumentValueAppender.Append(propertiesDocument, property.Key.ToString(), _bsonStructConverter.BsonString(property.Value.ToString()));
            }
            return propertiesDocument.ElementCount > 0 ? (BsonValue) propertiesDocument : BsonNull.Value;
        }
    }
}