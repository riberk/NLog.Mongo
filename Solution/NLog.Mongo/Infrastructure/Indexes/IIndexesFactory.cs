namespace NLog.Mongo.Infrastructure.Indexes
{
    using System.Threading.Tasks;
    using NLog.Mongo.Models;

    public interface IIndexesFactory
    {
        Task Create<T>(CreateIndexesContext<T> context);
    }
}