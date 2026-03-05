using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;
using CheckerBA.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace CheckerBA.Infrastructure.Repositories
{
    public class MongoEventRepository : IEventRepository
    {
        private readonly IMongoCollection<Event> _events;

        public MongoEventRepository(MongoDbContext mongoDbContext)
        {
            _events = mongoDbContext.Events;
        }

        public async Task AddEventAsync(Event events)
        {
            await _events.InsertOneAsync(events);
        }

        public async Task<List<Event>> GetEventsByDeviceIdAsync(string deviceId)
        {
            return await _events.Find(e => e.DeviceId == deviceId).ToListAsync();
        }
    }
}