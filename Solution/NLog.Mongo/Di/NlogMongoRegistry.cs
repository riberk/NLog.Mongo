namespace NLog.Mongo.Di
{
    using JetBrains.Annotations;
    using NLog.Mongo.Convert;
    using NLog.Mongo.Infrastructure;
    using NLog.Mongo.Infrastructure.Indexes;

    public static class NlogMongoRegistry
    {
        [NotNull] internal static readonly MongoTarget.IInternalLogger InternalLogger = new MongoTarget.InternalLoggerImpl();
        [NotNull] internal static readonly IConnectionStringRetriever ConnectionStringRetriever = new ConnectionStringRetriever();
        [NotNull] internal static readonly IBsonDocumentValueAppender BsonDocumentValueAppender = new BsonDocumentValueAppender();
        [NotNull] internal static readonly IBsonStructConverter BsonStructConverter = new BsonStructConverter();

        [NotNull] internal static readonly IBsonStructConvertMethodFactory BsonStructConvertMethodFactory =
            new BsonStructConvertMethodFactory(BsonStructConverter);

        [NotNull] internal static readonly IBsonConverter BsonConverter =
            new BsonConverter(BsonStructConverter, BsonStructConvertMethodFactory);

        [NotNull] internal static readonly IBsonExceptionFactory BsonExceptionFactory =
            new BsonExceptionFactory(BsonDocumentValueAppender, BsonStructConverter);

        [NotNull] internal static readonly IDefaultsFactory DefaultsFactory =
            new DefaultsFactory(BsonStructConverter, BsonExceptionFactory);

        [NotNull] internal static readonly IBsonPropertiesFactory BsonPropertiesFactory =
            new BsonPropertiesFactory(BsonConverter, BsonDocumentValueAppender, BsonStructConverter);

        [NotNull] internal static readonly IBsonDocumentCreator BsonDocumentCreator =
            new BsonDocumentCreator(BsonDocumentValueAppender, BsonConverter, DefaultsFactory, BsonPropertiesFactory);

        [NotNull] internal static IMongoCollectionResolver MongoCollectionResolver =
            new MongoCollectionResolver(new MongoCollectionResolver.CacheKeyFactory(),
                                        new MongoCollectionResolver.CollectionCreator(),
                                        new MongoCollectionResolver.MongoDatabaseFactory());

        [NotNull] internal static readonly IEventsWriter EventsWriter = new EventsWriter(BsonDocumentCreator, MongoCollectionResolver);
        [NotNull] internal static readonly IIndexesFactory IndexesFactory = new IndexesFactory(new IndexKeyFactory(), new OptionsMapper());
    }
}