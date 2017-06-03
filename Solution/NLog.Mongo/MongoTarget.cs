namespace NLog.Mongo
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using JetBrains.Annotations;
    using NLog.Common;
    using NLog.Config;
    using NLog.Mongo.Convert;
    using NLog.Mongo.Di;
    using NLog.Mongo.Infrastructure;
    using NLog.Targets;

    /// <summary>
    ///     NLog message target for MongoDB.
    /// </summary>
    [Target("Mongo")]
    public class MongoTarget : Target, IMongoTarget
    {
        [NotNull] private readonly IConnectionStringRetriever _connectionStringRetriever;
        [NotNull] private readonly IEventsWriter _eventsWriter;
        [NotNull] private readonly IInternalLogger _internalLogger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MongoTarget" /> class.
        /// </summary>
        public MongoTarget([NotNull] IConnectionStringRetriever connectionStringRetriever,
                           [NotNull] IEventsWriter eventsWriter,
                           [NotNull] IInternalLogger internalLogger)
        {
            if (connectionStringRetriever == null) throw new ArgumentNullException(nameof(connectionStringRetriever));
            if (eventsWriter == null) throw new ArgumentNullException(nameof(eventsWriter));
            if (internalLogger == null) throw new ArgumentNullException(nameof(internalLogger));
            IncludeDefaults = true;
            _connectionStringRetriever = connectionStringRetriever;
            _eventsWriter = eventsWriter;
            _internalLogger = internalLogger;
        }

        public MongoTarget() : this(NlogMongoRegistry.ConnectionStringRetriever, NlogMongoRegistry.EventsWriter, NlogMongoRegistry.InternalLogger)
        {
        }

        /// <summary>
        ///     Gets the fields collection.
        /// </summary>
        /// <value>
        ///     The fields.
        /// </value>
        [NotNull, ItemNotNull]
        [ArrayParameter(typeof(MongoField), "field")]
        public virtual IReadOnlyCollection<MongoField> Fields { get; } = new List<MongoField>();

        /// <summary>
        ///     Gets the properties collection.
        /// </summary>
        /// <value>
        ///     The properties.
        /// </value>
        [NotNull, ItemNotNull]
        [ArrayParameter(typeof(MongoField), "property")]
        public virtual IReadOnlyCollection<MongoField> Properties { get; } = new List<MongoField>();

        /// <summary>
        ///     Gets or sets the connection string name string.
        /// </summary>
        /// <value>
        ///     The connection name string.
        /// </value>
        public virtual string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the name of the connection.
        /// </summary>
        /// <value>
        ///     The name of the connection.
        /// </value>
        public virtual string ConnectionName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use the default document format.
        /// </summary>
        /// <value>
        ///     <c>true</c> to use the default document format; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IncludeDefaults { get; set; }

        /// <summary>
        ///     Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        ///     The name of the collection.
        /// </value>
        public virtual string CollectionName { get; set; }

        /// <summary>
        ///     Gets or sets the size in bytes of the capped collection.
        /// </summary>
        /// <value>
        ///     The size of the capped collection.
        /// </value>
        public virtual long? CappedCollectionSize { get; set; }

        /// <summary>
        ///     Gets or sets the capped collection max items.
        /// </summary>
        /// <value>
        ///     The capped collection max items.
        /// </value>
        public virtual long? CappedCollectionMaxItems { get; set; }

        /// <summary>
        ///     Initializes the target. Can be used by inheriting classes
        ///     to initialize logging.
        /// </summary>
        /// <exception cref="NLog.NLogConfigurationException">
        ///     Can not resolve MongoDB ConnectionString. Please make sure the
        ///     ConnectionString property is set.
        /// </exception>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                return;
            }
            if (string.IsNullOrEmpty(ConnectionName))
            {
                throw new NLogConfigurationException(
                        "Can not resolve MongoDB ConnectionString. Please make sure the ConnectionString property is set.");
            }
            ConnectionString = _connectionStringRetriever.GetConnectionString(ConnectionName);
        }

        /// <summary>
        ///     Writes an array of logging events to the log target. By default it iterates on all
        ///     events and passes them to "Write" method. Inheriting classes can use this method to
        ///     optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            if (logEvents == null || logEvents.Length == 0)
            {
                return;
            }

            try
            {
                _eventsWriter.Write(logEvents, this);

                foreach (var ev in logEvents)
                {
                    ev.Continuation(null);
                }
            }
            catch (Exception ex)
                    when (!(ex is StackOverflowException || ex is ThreadAbortException || ex is OutOfMemoryException || ex is NLogConfigurationException))
            {
                _internalLogger.Error("Error when writing to MongoDB {0}", ex);
                foreach (var ev in logEvents)
                {
                    ev.Continuation(ex);
                }
            }
        }

        public interface IInternalLogger
        {
            void Error(string message, Exception e);
        }

        internal class InternalLoggerImpl : IInternalLogger
        {
            public void Error(string message, Exception e)
            {
                InternalLogger.Error(message, e);
            }
        }
    }
}