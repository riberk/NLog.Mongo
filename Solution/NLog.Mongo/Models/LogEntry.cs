namespace NLog.Mongo.Models
{
    using System;
    using MongoDB.Bson;

    public class LogEntry
    {
        public ObjectId Id { get; set; }

        public DateTime? Date { get; set; }

        public string Level { get; set; }

        public string Logger { get; set; }

        public string Message { get; set; }

        public BsonDocument Properties { get; set; }

        public LogException Exception { get; set; }

        public string AnyText { get; set; }
    }
}