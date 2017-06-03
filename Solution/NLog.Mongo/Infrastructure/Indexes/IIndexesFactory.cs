namespace NLog.Mongo.Infrastructure.Indexes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public interface IIndexesFactory
    {
        Task Create<T>(CreateIndexesContext<T> context);
    }

    internal class IndexesFactory : IIndexesFactory
    {
        public async Task Create<T>(CreateIndexesContext<T> context)
        {
            IReadOnlyList<BsonDocument> indexes;
            using (var indexesCursor = await context.Collection.Indexes.ListAsync())
            {
                if (!await indexesCursor.MoveNextAsync())
                {
                    return;
                }
                indexes = indexesCursor.Current.ToList();
            }
            context.Collection.Indexes.CreateOneAsync(Builders<T>.IndexKeys.)
            throw new System.NotImplementedException();
        }
    }

    public enum CreationBehaviour
    {
        CreateNew = 1,
        CreateIfNotExists = 2,
        Replace = 3
    }

    public struct CreateIndexesContext<T>
    {
        public CreateIndexesContext([NotNull] IReadOnlyList<IIndexDefinition> definitions,
                                    [NotNull] IMongoCollection<T> collection)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Definitions = definitions;
            Collection = collection;
        }

        [NotNull]
        public IReadOnlyList<IIndexDefinition> Definitions { get; }

        [NotNull]
        public IMongoCollection<T> Collection { get; }
    }

    public interface IIndexDefinition
    {
        [NotNull]
        IMongoIndexOptions Options { get; }

        
    }
}