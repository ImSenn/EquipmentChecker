
using CheckerBA.Domain.Interfaces;
using CheckerBA.Domain.Entities;
using CheckerBA.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace CheckerBA.Infrastructure.Repositories
{
    public class MongoDeviceRepository : IDeviceRepository
    {
        private readonly IMongoCollection<Device> _devices;

        public MongoDeviceRepository(MongoDbContext mongoDbContext)
        {
            _devices = mongoDbContext.Devices;
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            return await _devices.Find(device => true).ToListAsync();
        }
        public async Task<Device> GetDeviceByIdAsync(string id)
        {
            return await _devices.Find(device => device.Id == id).FirstOrDefaultAsync();
        }
        public async Task AddDeviceAsync(Device device)
        {
            await _devices.InsertOneAsync(device);
        }

        public async Task UpdateDeviceAsync(Device device)
        {
            await _devices.ReplaceOneAsync(d => d.Id == device.Id, device, new ReplaceOptions { IsUpsert = true });
        }
        public async Task DeleteDeviceAsync(string id)
        {
            await _devices.DeleteOneAsync(d => d.Id == id);
        }
    }
}