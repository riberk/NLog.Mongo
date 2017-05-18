namespace NLog.Mongo
{
    using System.Collections.Generic;

    public interface IMongoWriteSettings
    {
        IReadOnlyCollection<MongoField> Fields { get; }
        bool IncludeDefaults { get; }
        IReadOnlyCollection<MongoField> Properties { get; }
    }
}