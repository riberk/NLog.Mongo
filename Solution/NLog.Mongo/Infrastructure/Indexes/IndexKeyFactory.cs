namespace NLog.Mongo.Infrastructure.Indexes
{
    using System;
    using MongoDB.Driver;

    internal class IndexKeyFactory : IIndexKeyFactory
    {
        public IndexKeysDefinition<T> Create<T>(FieldIndexType type, string fieldName)
        {
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            switch (type)
            {
                case FieldIndexType.Ascending:
                    return Builders<T>.IndexKeys.Ascending(new StringFieldDefinition<T>(fieldName));
                case FieldIndexType.Descending:
                    return Builders<T>.IndexKeys.Descending(new StringFieldDefinition<T>(fieldName));
                case FieldIndexType.GeoHaystack:
                    return Builders<T>.IndexKeys.GeoHaystack(new StringFieldDefinition<T>(fieldName));
                case FieldIndexType.Geo2D:
                    return Builders<T>.IndexKeys.Geo2D(new StringFieldDefinition<T>(fieldName));
                case FieldIndexType.Geo2DSphere:
                    return Builders<T>.IndexKeys.Geo2DSphere(new StringFieldDefinition<T>(fieldName));
                case FieldIndexType.Hashed:
                    return Builders<T>.IndexKeys.Hashed(new StringFieldDefinition<T>(fieldName));
                case FieldIndexType.Text:
                    return Builders<T>.IndexKeys.Text(new StringFieldDefinition<T>(fieldName));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}