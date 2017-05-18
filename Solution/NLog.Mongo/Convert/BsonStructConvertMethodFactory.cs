namespace NLog.Mongo.Convert
{
    using System;
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using NLog.Mongo.Infrastructure;

    internal class BsonStructConvertMethodFactory : IBsonStructConvertMethodFactory
    {
        [NotNull] private readonly IBsonStructConverter _converter;

        public BsonStructConvertMethodFactory([NotNull] IBsonStructConverter converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            _converter = converter;
        }

        public BsonTryConvertMethod Create(BsonType type)
        {
            switch (type)
            {
                case BsonType.Double:
                    return _converter.TryDouble;
                case BsonType.String:
                    return _converter.TryString;
                case BsonType.Boolean:
                    return _converter.TryBoolean;
                case BsonType.DateTime:
                    return _converter.TryDateTime;
                case BsonType.Int32:
                    return _converter.TryInt32;
                case BsonType.Int64:
                    return _converter.TryInt64;
                case BsonType.EndOfDocument:
                case BsonType.Document:
                case BsonType.Array:
                case BsonType.Binary:
                case BsonType.Undefined:
                case BsonType.ObjectId:
                case BsonType.Null:
                case BsonType.RegularExpression:
                case BsonType.JavaScript:
                case BsonType.Symbol:
                case BsonType.JavaScriptWithScope:
                case BsonType.Timestamp:
                case BsonType.MinKey:
                case BsonType.MaxKey:
                case BsonType.Decimal128:
                    throw new NotSupportedException($"Type {type} not supported");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}