using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;

namespace CheckerBA.Application.Services
{
    public class DeviceManagementService
    {
        private readonly IDeviceRepository _deviceRepo;

        public DeviceManagementService(IDeviceRepository deviceRepo)
        {
            _deviceRepo = deviceRepo;
        }

        public Task<List<Device>> GetAllDevicesAsync()
            => _deviceRepo.GetAllDevicesAsync();

        public Task<Device?> GetDeviceByIdAsync(string deviceId)
            => _deviceRepo.GetDeviceByIdAsync(deviceId);

        public async Task AddDeviceAsync(Device device)
        {
            device.CreatedAt = DateTime.UtcNow;
            await _deviceRepo.AddDeviceAsync(device);
        }

        public Task UpdateDeviceAsync(Device device)
            => _deviceRepo.UpdateDeviceAsync(device);

        public Task DeleteDeviceAsync(string deviceId)
            => _deviceRepo.DeleteDeviceAsync(deviceId);
    }
}