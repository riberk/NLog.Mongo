namespace NLog.Mongo.Convert
{
    using MongoDB.Bson;

    public interface IBsonConverter
    {
        BsonValue GetValue(MongoField field, LogEventInfo logEvent);
    }
}