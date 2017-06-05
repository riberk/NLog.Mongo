namespace NLog.Mongo.Infrastructure.Indexes
{
    using JetBrains.Annotations;
    using MongoDB.Driver;

    public interface IIndexKeyFactory
    {
        IndexKeysDefinition<T> Create<T>(FieldIndexType type, [NotNull] string fieldName);
    }
}