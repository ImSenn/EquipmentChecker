using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            return await _deviceRepo.GetAllDevicesAsync();
        }

        public async Task<Device> GetDeviceByIdAsync(string id)
        {
            return await _deviceRepo.GetDeviceByIdAsync(id);
        }

        public async Task AddDeviceAsync(Device device)
        {
            if (string.IsNullOrEmpty(device.Id))
            {
                device.Id = Guid.NewGuid().ToString();
            }
            device.CreatedAt = System.DateTime.UtcNow;
            await _deviceRepo.AddDeviceAsync(device);
        }

        public async Task UpdateDeviceAsync(Device device)
        {
            await _deviceRepo.UpdateDeviceAsync(device);
        }

        public async Task DeleteDeviceAsync(string id)
        {
            await _deviceRepo.DeleteDeviceAsync(id);
        }
    }
}