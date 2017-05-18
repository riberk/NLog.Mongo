namespace NLog.Mongo.Infrastructure
{
    using System;
    using MongoDB.Bson;

    public interface IBsonExceptionFactory
    {
        BsonValue Create(Exception exception);
    }
}