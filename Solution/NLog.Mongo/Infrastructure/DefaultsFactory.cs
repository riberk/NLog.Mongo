namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using NLog.Mongo.Convert;

    internal class DefaultsFactory : IDefaultsFactory
    {
        [NotNull] private readonly IBsonExceptionFactory _bsonExceptionFactory;
        [NotNull] private readonly IBsonStructConverter _bsonStructConverter;

        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="T:System.Object" />.
        /// </summary>
        public DefaultsFactory([NotNull] IBsonStructConverter bsonStructConverter,
                               [NotNull] IBsonExceptionFactory bsonExceptionFactory)
        {
            _bsonStructConverter = bsonStructConverter ?? throw new ArgumentNullException(nameof(bsonStructConverter));
            _bsonExceptionFactory = bsonExceptionFactory ?? throw new ArgumentNullException(nameof(bsonExceptionFactory));
        }

        public IEnumerable<KeyValuePair<string, BsonValue>> Create(LogEventInfo logEvent)
        {
            yield return new KeyValuePair<string, BsonValue>("Date", new BsonDateTime(logEvent.TimeStamp));
            yield return new KeyValuePair<string, BsonValue>("Level", _bsonStructConverter.BsonString(logEvent.Level?.Name));
            yield return new KeyValuePair<string, BsonValue>("Logger", _bsonStructConverter.BsonString(logEvent.LoggerName));
            yield return new KeyValuePair<string, BsonValue>("Message", _bsonStructConverter.BsonString(logEvent.FormattedMessage));
            yield return new KeyValuePair<string, BsonValue>("Exception", _bsonExceptionFactory.Create(logEvent.Exception));
        }
    }
}