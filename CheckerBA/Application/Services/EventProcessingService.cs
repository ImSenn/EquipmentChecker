using System.Collections.Generic;
using System.Threading.Tasks;
using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;

namespace CheckerBA.Application.Services
{
    public class EventProcessingService
    {
        private readonly IEventRepository _eventRepo;

        public EventProcessingService(IEventRepository eventRepo)
        {
            _eventRepo = eventRepo;
        }

        public async Task ProcessEventAsync(Event newEvent)
        {
            //code logic xử lý sự kiện ở đây, ví dụ: phân loại sự kiện, gửi thông báo, v.v.
            await _eventRepo.AddEventAsync(newEvent);
        }

        public async Task<List<Event>> GetDeviceEventsAsync(string deviceId)
        {
            return await _eventRepo.GetEventsByDeviceIdAsync(deviceId);
        }
    }
}