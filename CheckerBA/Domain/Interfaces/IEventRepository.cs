using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckerBA.Domain.Entities;

namespace CheckerBA.Domain.Interfaces
{
    public interface IEventRepository
    {
        Task AddEventAsync(Event events);
        Task<List<Event>> GetEventsByDeviceIdAsync(string deviceId);
    }
}