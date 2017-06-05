namespace NLog.Mongo.Infrastructure
{
    using System;
    using MongoDB.Bson;

    internal class BsonDocumentValueAppender : IBsonDocumentValueAppender
    {
        public void Append(BsonDocument document, string name, BsonValue value)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value != null && value != BsonNull.Value)
            {
                document.Add(name.Replace(".", @"_"), value);
            }
        }
    }
}