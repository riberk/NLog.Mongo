namespace NLog.Mongo.Infrastructure
{
    using System;
#if !CORE
    using System.Configuration;
#endif

    internal class ConnectionStringRetriever : IConnectionStringRetriever
    {
        public string GetConnectionString(string connectionName)
        {
#if CORE
            throw new NotSupportedException("Get connection string is not supported in .net standard");
#else
            if (connectionName == null) throw new ArgumentNullException(nameof(connectionName));

            var connectionString = ConfigurationManager.ConnectionStrings?[connectionName]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new NLogConfigurationException($"No connection string named '{connectionName}' found or it`s empty");
            }
            return connectionString;
#endif
        }
    }
}