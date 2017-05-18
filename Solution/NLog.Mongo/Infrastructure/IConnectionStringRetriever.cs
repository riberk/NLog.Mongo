namespace NLog.Mongo.Infrastructure
{
    public interface IConnectionStringRetriever
    {
        string GetConnectionString(string connectionName);
    }
}