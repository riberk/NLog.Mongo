namespace NLog.Mongo.Infrastructure
{
    using JetBrains.Annotations;
    using NLog.Common;

    public interface IEventsWriter
    {
        void Write([NotNull] AsyncLogEventInfo[] logEvents, [NotNull] IMongoTarget target);
    }
}