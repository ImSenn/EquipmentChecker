using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;
using CheckerBA.Infrastructure.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CheckerBA.Infrastructure.Repositories
{
    public class MongoTelemetryRepository : ITelemetryRepository
    {
        private readonly IMongoCollection<Telemetry> _telemetry;

        public MongoTelemetryRepository(MongoDbContext mongoDbContext)
        {
            _telemetry = mongoDbContext.Telemetries;
        }

        public async Task AddTelemetryAsync(Telemetry telemetry)
        {
            await _telemetry.InsertOneAsync(telemetry);
        }

        public async Task<List<Telemetry>> GetTelemetryHistoryAsync(string deviceId, DateTime StartTime, DateTime EndTime)
        {
            return await _telemetry.Find(t => t.DeviceId == deviceId && t.Timestamp >= StartTime && t.Timestamp <= EndTime)
                                   .SortByDescending(t => t.Timestamp)
                                   .ToListAsync();
        }
    }
}