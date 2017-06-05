namespace NLog.Mongo.Infrastructure.Indexes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class IndexesFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _indexKeyFactory = _mockFactory.Create<IIndexKeyFactory>();
            _optionsMapper = _mockFactory.Create<IOptionsMapper>();
            _indexesFactory = new IndexesFactory(_indexKeyFactory.Object, _optionsMapper.Object);
            _collection = _mockFactory.Create<IMongoCollection<BsonDocument>>();
            _indexManager = _mockFactory.Create<IMongoIndexManager<BsonDocument>>();
            _bsonSerializer = _mockFactory.Create<IBsonSerializer<BsonDocument>>();
            _bsonSerializerRegistry = _mockFactory.Create<IBsonSerializerRegistry>();
        }

        private MockRepository _mockFactory;
        private IndexesFactory _indexesFactory;
        private Mock<IIndexKeyFactory> _indexKeyFactory;
        private Mock<IOptionsMapper> _optionsMapper;
        private Mock<IMongoCollection<BsonDocument>> _collection;
        private Mock<IMongoIndexManager<BsonDocument>> _indexManager;
        private Mock<IBsonSerializer<BsonDocument>> _bsonSerializer;
        private Mock<IBsonSerializerRegistry> _bsonSerializerRegistry;

        [Test]
        public void Create_Exists_CreateNew()
        {
            const string indexName = "i1";
            MockExistsIndexes(new[] { indexName });
            var indexOption = _mockFactory.Create<IMongoIndexOptions>();
            indexOption.Setup(x => x.Name).Returns(indexName).Verifiable();
            indexOption.Setup(x => x.IndexCreationBehaviour).Returns(CreationBehaviour.CreateNew).Verifiable();
            var ctx = new CreateIndexesContext<BsonDocument>(new[] {indexOption.Object}, _collection.Object);
            Assert.ThrowsAsync<InvalidOperationException>(() => _indexesFactory.Create(ctx));
            _mockFactory.VerifyAll();
        }

        [Test]
        public async Task Create_Exists_CreateIfNotExists()
        {
            const string indexName = "i1";
            MockExistsIndexes(new[] { indexName });
            var indexOption = _mockFactory.Create<IMongoIndexOptions>();
            indexOption.Setup(x => x.Name).Returns(indexName).Verifiable();
            indexOption.Setup(x => x.IndexCreationBehaviour).Returns(CreationBehaviour.CreateIfNotExists).Verifiable();
            var ctx = new CreateIndexesContext<BsonDocument>(new[] { indexOption.Object }, _collection.Object);
            await _indexesFactory.Create(ctx);
            _mockFactory.VerifyAll();
        }

        [Test]
        public async Task Create_Exists_Replace()
        {
            const string indexName = "i1";
            MockExistsIndexes(new[] { indexName });

            const string field1Name = "field1";
            const string field2Name = "field2";

            var mockIndexResult = MockIndex(indexName,
                                            new[]
                                            {
                                                new Field(field1Name, FieldIndexType.Ascending),
                                                new Field(field2Name, FieldIndexType.Descending),
                                            });
            mockIndexResult.MongoIndexOptions.Setup(x => x.IndexCreationBehaviour).Returns(CreationBehaviour.Replace).Verifiable();
            _indexManager.Setup(x => x.DropOneAsync(indexName, default(CancellationToken))).Returns(Task.CompletedTask).Verifiable();
            _indexManager.Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<BsonDocument>>>(), default(CancellationToken)))
                         .ReturnsAsync((IEnumerable<CreateIndexModel<BsonDocument>> indexes, CancellationToken ct) =>
                         {
                             Assert.IsNotNull(indexes);
                             var indexesList = indexes.ToList();
                             Assert.AreEqual(1, indexesList.Count);
                             Assert.AreEqual(mockIndexResult.CreateIndexOptions, indexesList[0].Options);
                             CheckIndexBson(indexesList[0].Keys, field1Name, field2Name);
                             return new[] { indexName };
                         });
            var ctx = new CreateIndexesContext<BsonDocument>(new[] { mockIndexResult.MongoIndexOptions.Object }, _collection.Object);
            await _indexesFactory.Create(ctx);
            _mockFactory.VerifyAll();
        }

        [Test]
        public void Create_Exists_OutOfRange()
        {
            const string indexName = "i1";
            MockExistsIndexes(new[] { indexName });
            var indexOption = _mockFactory.Create<IMongoIndexOptions>();
            indexOption.Setup(x => x.Name).Returns(indexName).Verifiable();
            indexOption.Setup(x => x.IndexCreationBehaviour).Returns((CreationBehaviour)(-100)).Verifiable();
            var ctx = new CreateIndexesContext<BsonDocument>(new[] { indexOption.Object }, _collection.Object);
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _indexesFactory.Create(ctx));
            _mockFactory.VerifyAll();
        }




        [Test]
        public async Task Create_OneNotExistsIndex_Test()
        {
            MockExistsIndexes();

            const string field1Name = "field1";
            const string field2Name = "field2";

            const string indexName = "index_name";
            var mockIndexResult = MockIndex(indexName,
                      new[]
                      {
                          new Field(field1Name, FieldIndexType.Ascending),
                          new Field(field2Name, FieldIndexType.Descending),
                      });
            _indexManager.Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<BsonDocument>>>(), default(CancellationToken)))
                         .ReturnsAsync((IEnumerable<CreateIndexModel<BsonDocument>> indexes, CancellationToken ct) =>
                         {
                             Assert.IsNotNull(indexes);
                             var indexesList = indexes.ToList();
                             Assert.AreEqual(1, indexesList.Count);
                             Assert.AreEqual(mockIndexResult.CreateIndexOptions, indexesList[0].Options);
                             CheckIndexBson(indexesList[0].Keys, field1Name, field2Name);
                             return new[] {indexName};
                         });
            await _indexesFactory.Create(new CreateIndexesContext<BsonDocument>(new[] { mockIndexResult.MongoIndexOptions.Object}, _collection.Object));
            _mockFactory.VerifyAll();
            
        }

        private void MockExistsIndexes()
        {
            _collection.Setup(x => x.Indexes).Returns(_indexManager.Object).Verifiable();
            var cursor = _mockFactory.Create<IAsyncCursor<BsonDocument>>();
            _indexManager.Setup(x => x.ListAsync(default(CancellationToken))).ReturnsAsync(cursor.Object).Verifiable();
            cursor.Setup(x => x.MoveNextAsync(default(CancellationToken))).ReturnsAsync(false).Verifiable();
            cursor.As<IDisposable>().Setup(x => x.Dispose()).Verifiable();
        }

        private void MockExistsIndexes(IReadOnlyCollection<string> names)
        {
            _collection.Setup(x => x.Indexes).Returns(_indexManager.Object).Verifiable();
            var cursor = _mockFactory.Create<IAsyncCursor<BsonDocument>>();
            _indexManager.Setup(x => x.ListAsync(default(CancellationToken))).ReturnsAsync(cursor.Object).Verifiable();
            cursor.Setup(x => x.MoveNextAsync(default(CancellationToken))).ReturnsAsync(true).Verifiable();
            cursor.Setup(x => x.Current).Returns(names.Select(x => new BsonDocument(new Dictionary<string, object>(){{ "name", x } }))).Verifiable();
            cursor.As<IDisposable>().Setup(x => x.Dispose()).Verifiable();
        }

        private BsonDocument MockRender(Mock<IndexKeysDefinition<BsonDocument>> index, string fieldName)
        {
            var bd = new BsonDocument(new Dictionary<string, object>{{ fieldName, "value"}});
            index.Setup(x => x.Render(_bsonSerializer.Object, _bsonSerializerRegistry.Object)).Returns(bd).Verifiable();
            return bd;
        }

        private MockIndexResult MockIndex(string indexName, Field[] fields)
        {
            var indexFields = new List<IMongoIndexField>();
            foreach (var field in fields)
            {
                var f = _mockFactory.Create<IMongoIndexField>();
                f.Setup(x => x.Name).Returns(field.Name).Verifiable();
                f.Setup(x => x.Type).Returns(field.Type).Verifiable();
                indexFields.Add(f.Object);
                var fIndex = _mockFactory.Create<IndexKeysDefinition<BsonDocument>>();
                MockRender(fIndex, field.Name);
                _indexKeyFactory.Setup(x => x.Create<BsonDocument>(field.Type, field.Name)).Returns(fIndex.Object).Verifiable();
            }

            var indexOption = _mockFactory.Create<IMongoIndexOptions>();
            indexOption.Setup(x => x.Name).Returns(indexName).Verifiable();
            indexOption.Setup(x => x.IndexFields).Returns(indexFields);
            var createIndexOptions = new CreateIndexOptions<BsonDocument>();
            _optionsMapper.Setup(x => x.Map(indexOption.Object)).Returns(createIndexOptions).Verifiable();

            return new MockIndexResult(createIndexOptions, indexOption);
        }

        private void CheckIndexBson(IndexKeysDefinition<BsonDocument> definition, params string[] fieldNames)
        {
            var resBson = definition.Render(_bsonSerializer.Object, _bsonSerializerRegistry.Object);
            Assert.IsNotNull(resBson.Elements);
            var elements = resBson.Elements.ToDictionary(x => x.Name, x => x.Value.AsString);
            Assert.AreEqual(fieldNames.Length, elements.Count);
            foreach (var fieldName in fieldNames)
            {
                Assert.IsTrue(elements.ContainsKey(fieldName));
            }
        }

        private class Field
        {
            /// <summary>Инициализирует новый экземпляр класса <see cref="T:System.Object" />.</summary>
            public Field(string name, FieldIndexType type)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Type = type;
            }

            public string Name { get; }

            public FieldIndexType Type { get; }
        }

        private class MockIndexResult
        {
            /// <summary>Инициализирует новый экземпляр класса <see cref="T:System.Object" />.</summary>
            public MockIndexResult(CreateIndexOptions<BsonDocument> createIndexOptions, Mock<IMongoIndexOptions> mongoIndexOptions)
            {
                CreateIndexOptions = createIndexOptions ?? throw new ArgumentNullException(nameof(createIndexOptions));
                MongoIndexOptions = mongoIndexOptions ?? throw new ArgumentNullException(nameof(mongoIndexOptions));
            }

            public CreateIndexOptions<BsonDocument> CreateIndexOptions { get; }

            public Mock<IMongoIndexOptions> MongoIndexOptions { get; }
        }
        /*
               BsonDocument bsonDocument = new BsonDocument();
      foreach (IndexKeysDefinition<TDocument> key in this._keys)
      {
        foreach (BsonElement element in key.Render(documentSerializer, serializerRegistry).Elements)
        {
          if (bsonDocument.Contains(element.Name))
            throw new MongoException(string.Format("The index keys definition contains multiple values for the field '{0}'.", (object) element.Name));
          bsonDocument.Add(element);
        }
      }
      return bsonDocument;
         */
    }
}