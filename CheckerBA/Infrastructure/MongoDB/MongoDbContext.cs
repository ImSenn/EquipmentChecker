using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CheckerBA.Domain.Entities;
using System;

namespace CheckerBA.Infrastructure.MongoDB
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;

            var mongoSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
            mongoSettings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(settings.ServerSelectionTimeoutMs);
            mongoSettings.ConnectTimeout = TimeSpan.FromMilliseconds(settings.ConnectTimeoutMs);
            mongoSettings.SocketTimeout = TimeSpan.FromMilliseconds(settings.SocketTimeoutMs);

            var client = new MongoClient(mongoSettings);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<Device> Devices => _database.GetCollection<Device>("Devices");
        public IMongoCollection<Telemetry> Telemetries => _database.GetCollection<Telemetry>("Telemetries");
        public IMongoCollection<Event> Events => _database.GetCollection<Event>("Events");
        public IMongoCollection<EnergyUsed> EnergyUsed => _database.GetCollection<EnergyUsed>("EnergyUsed");
        public IMongoCollection<AppUser> Users => _database.GetCollection<AppUser>("Users");
    }
}