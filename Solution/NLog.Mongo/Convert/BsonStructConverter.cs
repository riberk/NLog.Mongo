namespace NLog.Mongo.Convert
{
    using System;
    using System.Globalization;
    using JetBrains.Annotations;
    using MongoDB.Bson;

    internal class BsonStructConverter : IBsonStructConverter
    {
        public bool TryBoolean(string value, out BsonValue bsonValue)
        {
            return TryT<bool>(value, bool.TryParse, b => new BsonBoolean(b), out bsonValue);
        }

        public bool TryDateTime(string value, out BsonValue bsonValue)
        {
            return TryT<DateTime>(value, DateTime.TryParse, b => new BsonDateTime(b), out bsonValue);
        }

        public bool TryDouble(string value, out BsonValue bsonValue)
        {
            return TryT(value, (string s, out double d) => double.TryParse(s.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out d),
                                b => new BsonDouble(b), out bsonValue);
        }

        public bool TryInt32(string value, out BsonValue bsonValue)
        {
            return TryT<int>(value, int.TryParse, b => new BsonInt32(b), out bsonValue);
        }

        public bool TryInt64(string value, out BsonValue bsonValue)
        {
            return TryT<long>(value, long.TryParse, b => new BsonInt64(b), out bsonValue);
        }

        public bool TryString(string value, out BsonValue bsonValue)
        {
            bsonValue = new BsonString(value);
            return true;
        }

        public BsonValue BsonString(string value)
        {
            return value != null ? (BsonValue) new BsonString(value) : BsonNull.Value;
        }

        private static bool TryT<T>(string value, [NotNull] TryParse<T> parser, [NotNull] Func<T, BsonValue> factory, out BsonValue bsonValue)
        {
            if (value == null)
            {
                bsonValue = default(BsonValue);
                return false;
            }
            T result;
            var parsed = parser(value, out result);
            bsonValue = parsed ? factory(result) : default(BsonValue);
            return parsed;
        }

        private delegate bool TryParse<T>(string s, out T value);
    }
}