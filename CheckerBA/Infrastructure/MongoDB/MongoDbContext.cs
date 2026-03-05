using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CheckerBA.Domain.Entities;

namespace CheckerBA.Infrastructure.MongoDB
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;

            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<Device> Devices => _database.GetCollection<Device>("Devices");
        public IMongoCollection<Telemetry> Telemetries => _database.GetCollection<Telemetry>("Telemetries");
        public IMongoCollection<Event> Events => _database.GetCollection<Event>("Events");
        public IMongoCollection<EnergyUsed> EnergyUsed => _database.GetCollection<EnergyUsed>("EnergyUsed");
    }
}