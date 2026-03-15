using CheckerBA.Domain.Entities;

namespace CheckerBA.Domain.Interfaces
{
    public interface IDeviceRepository
    {
        Task<List<Device>> GetAllDevicesAsync();
        Task<Device?> GetDeviceByIdAsync(string deviceId);
        Task AddDeviceAsync(Device device);
        Task UpdateDeviceAsync(Device device);
        Task DeleteDeviceAsync(string deviceId);
    }
}
