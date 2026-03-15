using Microsoft.AspNetCore.SignalR;

namespace CheckerBA.Hubs
{
    public class DeviceHub : Hub
    {
        /// <summary>Client gọi để nhận realtime update của một thiết bị cụ thể.</summary>
        public async Task JoinDeviceGroup(string deviceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, deviceId);
        }

        /// <summary>Client gọi để rời group của thiết bị.</summary>
        public async Task LeaveDeviceGroup(string deviceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, deviceId);
        }
    }
}
