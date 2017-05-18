namespace NLog.Mongo
{
    public interface IMongoConnectionSettings
    {
        string CollectionName { get; }
        string ConnectionName { get; }
        string ConnectionString { get; }
    }
}