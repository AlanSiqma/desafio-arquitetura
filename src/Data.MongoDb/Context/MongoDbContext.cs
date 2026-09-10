using Domain.Documents;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Data.MongoDb.Context
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString =
                configuration["MongoDB:ConnectionString"];

            var databaseName =
                configuration["MongoDB:Database"];

            var client = new MongoClient(connectionString);

            _database = client.GetDatabase(databaseName);
            CriarIndices();
        }


        public IMongoCollection<LancamentoDocument> Lancamentos =>
            _database.GetCollection<LancamentoDocument>("lancamentos");

        public IMongoCollection<ConsolidadoDiarioDocument> ConsolidadosDiarios =>
         _database.GetCollection<ConsolidadoDiarioDocument>(
             "consolidados_diarios");

        private void CriarIndices()
        {
            var indexKeys = Builders<ConsolidadoDiarioDocument>
                .IndexKeys
                .Ascending(x => x.Data);

            var indexOptions = new CreateIndexOptions
            {
                Unique = true
            };

            var indexModel = new CreateIndexModel<ConsolidadoDiarioDocument>(
                indexKeys,
                indexOptions);

            ConsolidadosDiarios.Indexes.CreateOne(indexModel);
        }

        public async Task PingAsync(CancellationToken cancellationToken = default)
        {
            await _database
                .RunCommandAsync<BsonDocument>(
                    new BsonDocument("ping", 1),
                    cancellationToken: cancellationToken);
        }
    }
}
