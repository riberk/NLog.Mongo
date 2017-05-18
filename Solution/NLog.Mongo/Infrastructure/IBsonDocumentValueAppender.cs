namespace NLog.Mongo.Infrastructure
{
    using JetBrains.Annotations;
    using MongoDB.Bson;

    public interface IBsonDocumentValueAppender
    {
        void Append([NotNull] BsonDocument document, [NotNull] string name, BsonValue value);
    }
}