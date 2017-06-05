namespace NLog.Mongo.Convert
{
    using System;
    using JetBrains.Annotations;
    using MongoDB.Bson;

    /// <summary>
    ///     Convert string values to Mongo <see cref="BsonValue" />.
    /// </summary>
    internal class BsonConverter : IBsonConverter
    {
        [NotNull] private readonly IBsonStructConverter _bsonStructConverter;
        [NotNull] private readonly IBsonStructConvertMethodFactory _bsonStructConvertMethodFactory;

        public BsonConverter([NotNull] IBsonStructConverter bsonStructConverter, [NotNull] IBsonStructConvertMethodFactory bsonStructConvertMethodFactory)
        {
            _bsonStructConverter = bsonStructConverter ?? throw new ArgumentNullException(nameof(bsonStructConverter));
            _bsonStructConvertMethodFactory = bsonStructConvertMethodFactory ?? throw new ArgumentNullException(nameof(bsonStructConvertMethodFactory));
        }

        public BsonValue GetValue(MongoField field, LogEventInfo logEvent)
        {
            var value = field?.Layout.Render(logEvent)?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            BsonType type;
            BsonValue bsonValue;
            if (!Enum.TryParse(field.BsonType, true, out type) || !_bsonStructConvertMethodFactory.Create(type)(value, out bsonValue))
            {
                bsonValue = _bsonStructConverter.BsonString(value);
            }
            return bsonValue;
        }
    }
}