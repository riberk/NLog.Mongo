using System.Collections.Generic;
using JetBrains.Annotations;
using NLog.Common;

namespace NLog.Mongo.Infrastructure
{
    

    public interface IEventsWriter
    {
        void Write([NotNull] IEnumerable<AsyncLogEventInfo> logEvents, [NotNull] IMongoTarget target);
    }
}