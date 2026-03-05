using System.Collections.Generic;
using System.Threading.Tasks;
using CheckerBA.Domain.Entities;

namespace CheckerBA.Domain.Interfaces
{
    public interface ITelemetryRepository
    {
        Task AddTelemetryAsync(Telemetry telemetry);
        Task<List<Telemetry>> GetTelemetryHistoryAsync(string deviceId, DateTime StartTime, DateTime EndTime);
    }
}