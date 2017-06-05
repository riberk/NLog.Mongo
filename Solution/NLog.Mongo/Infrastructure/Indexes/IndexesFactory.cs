namespace NLog.Mongo.Infrastructure.Indexes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using MongoDB.Driver;

    internal class IndexesFactory : IIndexesFactory
    {
        [NotNull] private readonly IIndexKeyFactory _indexKeyFactory;
        [NotNull] private readonly IOptionsMapper _optionsMapper;

        public IndexesFactory([NotNull] IIndexKeyFactory indexKeyFactory, [NotNull] IOptionsMapper optionsMapper)
        {
            _indexKeyFactory = indexKeyFactory ?? throw new ArgumentNullException(nameof(indexKeyFactory));
            _optionsMapper = optionsMapper ?? throw new ArgumentNullException(nameof(optionsMapper));
        }

        public async Task Create<T>(CreateIndexesContext<T> context)
        {
            HashSet<string> existsIndexes;
            using (var indexesCursor = await context.Collection.Indexes.ListAsync())
            {
                if (await indexesCursor.MoveNextAsync())
                {
                    existsIndexes = new HashSet<string>(indexesCursor.Current.Select(x => x["name"].AsString));
                }
                else
                {
                    existsIndexes = new HashSet<string>();
                }
            }
            var creatingIndexes = new List<CreateIndexModel<T>>();
            var replacingIndexes = new List<string>();
            foreach (var indexOptions in context.Indexes)
            {
                if (existsIndexes.Contains(indexOptions.Name))
                {
                    switch (indexOptions.IndexCreationBehaviour)
                    {
                        case CreationBehaviour.CreateNew:
                            throw new InvalidOperationException("Index exists. Use creationBehaviour option");
                        case CreationBehaviour.CreateIfNotExists:
                            continue;
                        case CreationBehaviour.Replace:
                            replacingIndexes.Add(indexOptions.Name);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                var indexKeysDefinitions = indexOptions.IndexFields.Select(x => _indexKeyFactory.Create<T>(x.Type, x.Name));
                var indexKeysDefinition = Builders<T>.IndexKeys.Combine(indexKeysDefinitions);
                creatingIndexes.Add(new CreateIndexModel<T>(indexKeysDefinition, _optionsMapper.Map(indexOptions)));
            }
            foreach (var deletingIndex in replacingIndexes)
            {
                //TODO emulate transaction
                await context.Collection.Indexes.DropOneAsync(deletingIndex);
            }
            if (creatingIndexes.Any())
            {
                var res = await context.Collection.Indexes.CreateManyAsync(creatingIndexes);
            }
        }
    }
}