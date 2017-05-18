namespace NLog.Mongo.Infrastructure
{
    using MongoDB.Bson;

    public delegate bool BsonTryConvertMethod(string value, out BsonValue converted);
}