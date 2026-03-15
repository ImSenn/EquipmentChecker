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
            return await _devices.Find(_ => true).ToListAsync();
        }

        public async Task<Device?> GetDeviceByIdAsync(string deviceId)
        {
            return await _devices.Find(d => d.DeviceId == deviceId).FirstOrDefaultAsync();
        }

        public async Task AddDeviceAsync(Device device)
        {
            await _devices.InsertOneAsync(device);
        }

        public async Task UpdateDeviceAsync(Device device)
        {
            await _devices.ReplaceOneAsync(d => d.DeviceId == device.DeviceId, device, new ReplaceOptions { IsUpsert = false });
        }

        public async Task DeleteDeviceAsync(string deviceId)
        {
            await _devices.DeleteOneAsync(d => d.DeviceId == deviceId);
        }
    }
}