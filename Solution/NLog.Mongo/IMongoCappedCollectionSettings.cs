namespace NLog.Mongo
{
    public interface IMongoCappedCollectionSettings
    {
        long? CappedCollectionMaxItems { get; }
        long? CappedCollectionSize { get; }
    }
}