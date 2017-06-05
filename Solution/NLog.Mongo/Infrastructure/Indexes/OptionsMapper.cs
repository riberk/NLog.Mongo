namespace NLog.Mongo.Infrastructure.Indexes
{
    using MongoDB.Driver;

    internal class OptionsMapper : IOptionsMapper
    {
        public CreateIndexOptions Map(IMongoIndexOptions options)
        {
            return new CreateIndexOptions
            {
                Name = options.Name,
                BucketSize = options.BucketSize,
                //Collation = TODO,
                Background = options.Background,
                Bits = options.Bits,
                DefaultLanguage = options.DefaultLanguage,
                LanguageOverride = options.LanguageOverride,
                Max = options.Max,
                Min = options.Min,
                Sparse = options.Sparse,
                SphereIndexVersion = options.SphereIndexVersion,
                //StorageEngine = TODO,
                TextIndexVersion = options.TextIndexVersion,
                Unique = options.Unique,
                Version = options.Version,
                //Weights = TODO
            };
        }
    }
}