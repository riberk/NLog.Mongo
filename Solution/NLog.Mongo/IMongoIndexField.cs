namespace NLog.Mongo
{
    using JetBrains.Annotations;

    public interface IMongoIndexField
    {
        [NotNull]
        string Name { get; }
        FieldIndexType Type { get; }
    }
}