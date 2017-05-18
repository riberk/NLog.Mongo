namespace NLog.Mongo.Infrastructure
{
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public interface IMongoCollectionResolver
    {
        IMongoCollection<BsonDocument> GetCollection([NotNull] IMongoSettings mongoTarget);
    }
}