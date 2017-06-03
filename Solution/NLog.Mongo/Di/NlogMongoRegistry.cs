namespace NLog.Mongo.Di
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using NLog.Mongo.Convert;
    using NLog.Mongo.Infrastructure;

    public static class NlogMongoRegistry
    {
        [NotNull] private static readonly Dictionary<Type, Type> InternalBindings;

        [NotNull]
        public static IReadOnlyDictionary<Type, Type> Bindings => InternalBindings;

        static NlogMongoRegistry()
        {
            InternalBindings = new Dictionary<Type, Type>();
            Add<IBsonConverter, BsonConverter>();
            Add<IBsonStructConverter, BsonStructConverter>();
            Add<IBsonStructConvertMethodFactory, BsonStructConvertMethodFactory>();
            Add<IDefaultsFactory, DefaultsFactory>();
            Add<IBsonExceptionFactory, BsonExceptionFactory>();
            Add<IBsonDocumentCreator, BsonDocumentCreator>();
            Add<IBsonDocumentValueAppender, BsonDocumentValueAppender>();
            Add<IMongoCollectionResolver, MongoCollectionResolver>();
            Add<IBsonPropertiesFactory, BsonPropertiesFactory>();
            Add<IConnectionStringRetriever, ConnectionStringRetriever>();
            Add<IEventsWriter, EventsWriter>();
            Add<MongoCollectionResolver.ICacheKeyFactory, MongoCollectionResolver.CacheKeyFactory>();
            Add<MongoCollectionResolver.ICollectionCreator, MongoCollectionResolver.CollectionCreator>();
            Add<MongoCollectionResolver.IMongoDatabaseFactory, MongoCollectionResolver.MongoDatabaseFactory>();
            Add<MongoTarget.IInternalLogger, MongoTarget.InternalLoggerImpl>();
        }

        private static void Add<TFrom, TTo>()
        {
            InternalBindings.Add(typeof(TFrom), typeof(TTo));
        }

        [NotNull] internal static readonly MongoTarget.IInternalLogger InternalLogger = new MongoTarget.InternalLoggerImpl();
        [NotNull] internal static readonly IConnectionStringRetriever ConnectionStringRetriever = new ConnectionStringRetriever();
        [NotNull] internal static readonly IBsonDocumentValueAppender BsonDocumentValueAppender = new BsonDocumentValueAppender();
        [NotNull] internal static readonly IBsonStructConverter BsonStructConverter = new BsonStructConverter();
        [NotNull] internal static readonly IBsonStructConvertMethodFactory BsonStructConvertMethodFactory = new BsonStructConvertMethodFactory(BsonStructConverter);
        [NotNull] internal static readonly IBsonConverter BsonConverter = new BsonConverter(BsonStructConverter, BsonStructConvertMethodFactory);
        [NotNull] internal static readonly IBsonExceptionFactory BsonExceptionFactory = new BsonExceptionFactory(BsonDocumentValueAppender, BsonStructConverter);
        [NotNull] internal static readonly IDefaultsFactory DefaultsFactory = new DefaultsFactory(BsonStructConverter, BsonExceptionFactory);
        [NotNull] internal static readonly IBsonPropertiesFactory BsonPropertiesFactory = new BsonPropertiesFactory(BsonConverter, BsonDocumentValueAppender, BsonStructConverter);
        [NotNull] internal static readonly IBsonDocumentCreator BsonDocumentCreator = new BsonDocumentCreator(BsonDocumentValueAppender, BsonConverter, DefaultsFactory, BsonPropertiesFactory);

        [NotNull] internal static IMongoCollectionResolver MongoCollectionResolver =
            new MongoCollectionResolver(new MongoCollectionResolver.CacheKeyFactory(),
                                        new MongoCollectionResolver.CollectionCreator(),
                                        new MongoCollectionResolver.MongoDatabaseFactory());
        [NotNull] internal static readonly IEventsWriter EventsWriter = new EventsWriter(BsonDocumentCreator, MongoCollectionResolver);



        //new ConnectionStringRetriever(), new EventsWriter(new BsonDocumentCreator(new BsonDocumentValueAppender(), new BsonConverter(new BsonStructConverter(), new BsonStructConvertMethodFactory(new BsonStructConverter()), new ), ), ), 
    }
}