using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;
using CheckerBA.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace CheckerBA.Infrastructure.Repositories
{
    public class MongoEnergyUsedRepository : IEnergyUsedRepository
    {
        private readonly IMongoCollection<EnergyUsed> _energyUsed;

        public MongoEnergyUsedRepository(MongoDbContext mongoDbContext)
        {
            _energyUsed = mongoDbContext.EnergyUsed;
        }

        public async Task<List<EnergyUsed>> GetEnergyUsedByDeviceIdAsync(string deviceId, DateTime date)
        {
            return await _energyUsed.Find(e => e.DeviceId == deviceId && e.date == date)
                                    .ToListAsync();
        }

        public async Task UpsertEnergyUsedListAsync(List<EnergyUsed> energyUsedList)
        {
            var models = new List<WriteModel<EnergyUsed>>();
            foreach (var energyUsed in energyUsedList)
            {
                var filter = Builders<EnergyUsed>.Filter.Eq(e => e.DeviceId, energyUsed.DeviceId) &
                             Builders<EnergyUsed>.Filter.Eq(e => e.date, energyUsed.date);
                var upsertOne = new ReplaceOneModel<EnergyUsed>(filter, energyUsed) { IsUpsert = true };
                models.Add(upsertOne);
            }
            await _energyUsed.BulkWriteAsync(models);
        }

        public async Task UpsertEnergyUsedAsync(EnergyUsed energyUsed)
        {
            var filter = Builders<EnergyUsed>.Filter.Eq(e => e.DeviceId, energyUsed.DeviceId) &
                         Builders<EnergyUsed>.Filter.Eq(e => e.date, energyUsed.date);
            var options = new ReplaceOptions { IsUpsert = true };

            await _energyUsed.ReplaceOneAsync(filter, energyUsed, options);
        }

        public async Task<EnergyUsed> GetDeviceEnergyUsedByIdAsync(string deviceId, DateTime date)
        {
            var hasEnergyRecord = await _energyUsed
                .Find(e => e.DeviceId == deviceId && e.date == date)
                .AnyAsync();

            if (!hasEnergyRecord)
            {
                throw new KeyNotFoundException($"No energy usage found for device '{deviceId}' on {date:yyyy-MM-dd}.");
            }

            var energyUsed = await _energyUsed.Find(e => e.DeviceId == deviceId && e.date == date)
                                              .FirstOrDefaultAsync();

            if (energyUsed is null)
            {
                throw new KeyNotFoundException($"Device '{deviceId}' was not found.");
            }

            return energyUsed;
        }
    }
}