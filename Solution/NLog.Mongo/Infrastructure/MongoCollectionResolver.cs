namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using NLog.Mongo.Internal;

    internal class MongoCollectionResolver : IMongoCollectionResolver
    {
        [NotNull, ItemNotNull] private static readonly ConcurrentDictionary<string, IMongoCollection<BsonDocument>> CollectionCache =
                new ConcurrentDictionary<string, IMongoCollection<BsonDocument>>();

        [NotNull] private readonly ICacheKeyFactory _cacheKeyFactory;
        [NotNull] private readonly ICollectionCreator _collectionCreator;
        [NotNull] private readonly IMongoDatabaseFactory _mongoDatabaseFactory;

        public MongoCollectionResolver([NotNull] ICacheKeyFactory cacheKeyFactory,
                                       [NotNull] ICollectionCreator collectionCreator,
                                       [NotNull] IMongoDatabaseFactory mongoDatabaseFactory)
        {
            if (cacheKeyFactory == null) throw new ArgumentNullException(nameof(cacheKeyFactory));
            if (collectionCreator == null) throw new ArgumentNullException(nameof(collectionCreator));
            if (mongoDatabaseFactory == null) throw new ArgumentNullException(nameof(mongoDatabaseFactory));
            _cacheKeyFactory = cacheKeyFactory;
            _collectionCreator = collectionCreator;
            _mongoDatabaseFactory = mongoDatabaseFactory;
        }

        [NotNull]
        public IMongoCollection<BsonDocument> GetCollection(IMongoSettings mongoTarget)
        {
            if (mongoTarget == null) throw new ArgumentNullException(nameof(mongoTarget));
            // ReSharper disable once AssignNullToNotNullAttribute
            return CollectionCache.GetOrAdd(_cacheKeyFactory.Create(mongoTarget), k =>
            {
                var database = _mongoDatabaseFactory.Create(mongoTarget.ConnectionString);
                if (database == null)
                {
                    throw new InvalidOperationException("Not found mongo db and can not create it");
                }
                return AsyncHelper.RunSync(() => _collectionCreator.CheckAndCreate(mongoTarget, database));
            });
        }

        internal interface IMongoDatabaseFactory
        {
            IMongoDatabase Create([NotNull] string connectionString);
        }

        internal class MongoDatabaseFactory : IMongoDatabaseFactory
        {
            public IMongoDatabase Create(string connectionString)
            {
                if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
                var mongoUrl = new MongoUrl(connectionString);
                var client = new MongoClient(mongoUrl);
                return client.GetDatabase(mongoUrl.DatabaseName ?? "NLog");
            }
        }

        internal interface ICollectionCreator
        {
            Task<IMongoCollection<BsonDocument>> CheckAndCreate([NotNull] IMongoSettings settings, [NotNull] IMongoDatabase database);
        }

        internal class CollectionCreator : ICollectionCreator
        {
            public async Task<IMongoCollection<BsonDocument>> CheckAndCreate(IMongoSettings settings, IMongoDatabase database)
            {
                if (settings == null) throw new ArgumentNullException(nameof(settings));
                if (database == null) throw new ArgumentNullException(nameof(database));
                var collectionName = settings.CollectionName ?? "Log";
                if (settings.CappedCollectionSize.HasValue && !await CollectionExistsAsync(database, collectionName))
                {
                    var options = new CreateCollectionOptions
                    {
                        Capped = true,
                        MaxSize = settings.CappedCollectionSize.Value
                    };
                    if (settings.CappedCollectionMaxItems.HasValue)
                    {
                        options.MaxDocuments = settings.CappedCollectionMaxItems.Value;
                    }

                    await database.CreateCollectionAsync(collectionName, options);
                }
                return database.GetCollection<BsonDocument>(collectionName);
            }

            private static async Task<bool> CollectionExistsAsync([NotNull] IMongoDatabase database, [NotNull] string collectionName)
            {
                var filter = new BsonDocument("name", collectionName);
                var collections = await database.ListCollectionsAsync(new ListCollectionsOptions {Filter = filter});
                return await collections.AnyAsync();
            }
        }

        internal interface ICacheKeyFactory
        {
            [NotNull]
            string Create([NotNull] IMongoConnectionSettings settings);
        }

        internal class CacheKeyFactory : ICacheKeyFactory
        {
            public string Create(IMongoConnectionSettings settings)
            {
                return $"k|{settings.ConnectionName ?? string.Empty}|{settings.ConnectionString ?? string.Empty}|{settings.CollectionName ?? string.Empty}";
            }
        }
    }
}