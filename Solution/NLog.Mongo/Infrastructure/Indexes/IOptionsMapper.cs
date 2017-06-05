namespace NLog.Mongo.Infrastructure.Indexes
{
    using JetBrains.Annotations;
    using MongoDB.Driver;

    public interface IOptionsMapper
    {
        CreateIndexOptions Map([NotNull] IMongoIndexOptions options);
    }
}