namespace NLog.Mongo.Ninject
{
    using global::Ninject.Modules;
    using NLog.Mongo.Convert;
    using NLog.Mongo.Infrastructure;

    public class NlogMongoModule : NinjectModule
    {
        /// <summary>
        ///     Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            Bind<IBsonConverter>().To<BsonConverter>().InSingletonScope();
            Bind<IBsonStructConverter>().To<BsonStructConverter>().InSingletonScope();
            Bind<IBsonStructConvertMethodFactory>().To<BsonStructConvertMethodFactory>().InSingletonScope();

            Bind<IDefaultsFactory>().To<DefaultsFactory>().InSingletonScope();
            Bind<IBsonExceptionFactory>().To<BsonExceptionFactory>().InSingletonScope();
            Bind<IBsonDocumentCreator>().To<BsonDocumentCreator>().InSingletonScope();
            Bind<IBsonDocumentValueAppender>().To<BsonDocumentValueAppender>().InSingletonScope();
            Bind<IMongoCollectionResolver>().To<MongoCollectionResolver>().InSingletonScope();
            Bind<IBsonPropertiesFactory>().To<BsonPropertiesFactory>().InSingletonScope();
            Bind<IConnectionStringRetriever>().To<ConnectionStringRetriever>().InSingletonScope();
            Bind<IEventsWriter>().To<EventsWriter>().InSingletonScope();
            Bind<MongoCollectionResolver.ICacheKeyFactory>().To<MongoCollectionResolver.CacheKeyFactory>().InSingletonScope();
            Bind<MongoCollectionResolver.ICollectionCreator>().To<MongoCollectionResolver.CollectionCreator>().InSingletonScope();
            Bind<MongoCollectionResolver.IMongoDatabaseFactory>().To<MongoCollectionResolver.MongoDatabaseFactory>().InSingletonScope();
            Bind<MongoTarget.IInternalLogger>().To<MongoTarget.InternalLoggerImpl>().InSingletonScope();
        }
    }
}