﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MongoDB.Bson;
using NLog.Common;
using NLog.Config;
using NLog.Mongo.Di;
using NLog.Mongo.Infrastructure;
using NLog.Mongo.Infrastructure.Indexes;
using NLog.Mongo.Internal;
using NLog.Targets;

namespace NLog.Mongo
{
    /// <summary>
    ///     NLog message target for MongoDB.
    /// </summary>
    [Target("Mongo")]
    public class MongoTarget : Target, IMongoTarget
    {
        [NotNull] private readonly IConnectionStringRetriever _connectionStringRetriever;
        [NotNull] private readonly IEventsWriter _eventsWriter;
        [NotNull] private readonly IIndexesFactory _indexesFactory;
        [NotNull] private readonly IMongoCollectionResolver _mongoCollectionResolver;
        [NotNull] private readonly IInternalLogger _internalLogger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MongoTarget" /> class.
        /// </summary>
        public MongoTarget([NotNull] IConnectionStringRetriever connectionStringRetriever,
                           [NotNull] IEventsWriter eventsWriter,
                           [NotNull] IIndexesFactory indexesFactory,
                           [NotNull] IMongoCollectionResolver mongoCollectionResolver,
                           [NotNull] IInternalLogger internalLogger)
        {
            IncludeDefaults = true;
            _connectionStringRetriever = connectionStringRetriever ?? throw new ArgumentNullException(nameof(connectionStringRetriever));
            _eventsWriter = eventsWriter ?? throw new ArgumentNullException(nameof(eventsWriter));
            _indexesFactory = indexesFactory ?? throw new ArgumentNullException(nameof(indexesFactory));
            _mongoCollectionResolver = mongoCollectionResolver ?? throw new ArgumentNullException(nameof(mongoCollectionResolver));
            _internalLogger = internalLogger ?? throw new ArgumentNullException(nameof(internalLogger));
        }

        public MongoTarget() : this(
            NlogMongoRegistry.ConnectionStringRetriever, 
            NlogMongoRegistry.EventsWriter,
            NlogMongoRegistry.IndexesFactory,
            NlogMongoRegistry.MongoCollectionResolver,
            NlogMongoRegistry.InternalLogger)
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
        ///     Gets the properties collection.
        /// </summary>
        /// <value>
        ///     The properties.
        /// </value>
        [NotNull, ItemNotNull]
        [ArrayParameter(typeof(MongoIndex), "index")]
        public virtual IReadOnlyCollection<MongoIndex> Indexes { get; } = new List<MongoIndex>();

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

        public virtual long? CappedCollectionMaxItems { get; set; }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
            }
            else if (string.IsNullOrEmpty(ConnectionName))
            {
                throw new NLogConfigurationException(
                                                     "Can not resolve MongoDB ConnectionString. Please make sure the ConnectionString property is set.");
            }
            else
            {
                ConnectionString = _connectionStringRetriever.GetConnectionString(ConnectionName);
            }
            AsyncHelper.RunSync(async () =>
            {
                var collection = _mongoCollectionResolver.GetCollection(this);
                await _indexesFactory.Create(new CreateIndexesContext<BsonDocument>(Indexes, collection));
            });
        }

        
        
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents == null || logEvents.Count == 0)
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
                    when (!(
#if !CORE
                    ex is StackOverflowException || ex is ThreadAbortException || 
#endif
                    ex is OutOfMemoryException || ex is NLogConfigurationException))
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