namespace NLog.Mongo.Convert
{
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using NLog.Mongo.Infrastructure;

    public interface IBsonStructConvertMethodFactory
    {
        [NotNull]
        BsonTryConvertMethod Create(BsonType type);
    }
}