namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Configuration;

    internal class ConnectionStringRetriever : IConnectionStringRetriever
    {
        public string GetConnectionString(string connectionName)
        {
            if (connectionName == null) throw new ArgumentNullException(nameof(connectionName));
            var settings = ConfigurationManager.ConnectionStrings?[connectionName];
            if (string.IsNullOrWhiteSpace(settings?.ConnectionString))
            {
                throw new NLogConfigurationException($"No connection string named '{connectionName}' found or it`s empty");
            }
            return settings.ConnectionString;
        }
    }
}