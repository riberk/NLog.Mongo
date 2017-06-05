using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polygon
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using NLog;
    using NLog.Mongo;
    using NLog.Targets.Wrappers;

    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetLogger("123");
            var target = (MongoTarget) ((AsyncTargetWrapper) LogManager.Configuration.FindTargetByName("MongoDb")).WrappedTarget;

            var url = new MongoUrl("mongodb://localhost/NlogMongoPolygon");
            var client = new MongoClient(url);
            var database = client.GetDatabase("NlogMongoPolygon");
            var col = database.GetCollection<BsonDocument>("Logs");
            using (var cursor = col.Indexes.List())
            {
                cursor.MoveNext();
                var indexes = cursor.Current.ToList();
            }
        }
    }
}
