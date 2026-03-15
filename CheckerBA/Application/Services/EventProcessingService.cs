using CheckerBA.Application.DTOs;
using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;
using CheckerBA.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CheckerBA.Application.Services
{
    public class EventProcessingService
    {
        private readonly IEventRepository _eventRepo;
        private readonly IHubContext<DeviceHub> _hubContext;

        public EventProcessingService(IEventRepository eventRepo, IHubContext<DeviceHub> hubContext)
        {
            _eventRepo = eventRepo;
            _hubContext = hubContext;
        }

        public async Task ProcessEventAsync(Event newEvent)
        {
            // 1. Lưu vào MongoDB
            await _eventRepo.AddEventAsync(newEvent);

            // 2. Broadcast alert tới tất cả clients (và group của device)
            var alert = new AlertDto(
                newEvent.DeviceId,
                newEvent.events.Type,
                newEvent.events.severity,
                newEvent.events.value,
                newEvent.events.message,
                newEvent.Timestamp);

            await _hubContext.Clients.All.SendAsync("ReceiveAlert", alert);
        }

        public Task<List<Event>> GetDeviceEventsAsync(string deviceId)
            => _eventRepo.GetEventsByDeviceIdAsync(deviceId);
    }
}