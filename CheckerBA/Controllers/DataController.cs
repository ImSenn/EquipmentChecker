using CheckerBA.Application.DTOs;
using CheckerBA.Application.Services;
using CheckerBA.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckerBA.Controllers
{
    /// <summary>Telemetry, Events, Energy – truy vấn lịch sử cho một device</summary>
    [ApiController]
    [Route("api/devices/{deviceId}")]
    [Authorize]
    public class DataController : ControllerBase
    {
        private readonly TelemetryProcessingService _telemetryService;
        private readonly EventProcessingService _eventService;
        private readonly IEnergyUsedRepository _energyRepo;

        public DataController(
            TelemetryProcessingService telemetryService,
            EventProcessingService eventService,
            IEnergyUsedRepository energyRepo)
        {
            _telemetryService = telemetryService;
            _eventService = eventService;
            _energyRepo = energyRepo;
        }

        // ── GET /api/devices/{deviceId}/telemetry?from=&to=&limit= ────────
        [HttpGet("telemetry")]
        public async Task<ActionResult<List<TelemetryDto>>> GetTelemetry(
            string deviceId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int limit = 100)
        {
            var start = from ?? DateTime.UtcNow.AddHours(-24);
            var end = to ?? DateTime.UtcNow;

            var items = await _telemetryService.GetTelemetryHistoryAsync(deviceId, start, end);

            var dtos = items.Take(limit).Select(t => new TelemetryDto(
                t.DeviceId,
                t.Timestamp,
                new MetricsDto(t.Metrics.Temperature, t.Metrics.Vibration, t.Metrics.estimatedPower),
                new StatusDto(t.Status.runState, t.Status.powerState),
                new SystemDto(t.System.upTime, t.System.wifiRssi, t.System.freeHeap)));

            return Ok(dtos);
        }

        // ── GET /api/devices/{deviceId}/events ────────────────────────────
        [HttpGet("events")]
        public async Task<ActionResult<List<EventDto>>> GetEvents(string deviceId)
        {
            var items = await _eventService.GetDeviceEventsAsync(deviceId);

            var dtos = items.Select(e => new EventDto(
                e.DeviceId,
                e.Timestamp,
                e.events.Type,
                e.events.severity,
                e.events.value,
                e.events.message));

            return Ok(dtos);
        }

        // ── GET /api/devices/{deviceId}/energy?date=2026-03-15 ────────────
        [HttpGet("energy")]
        public async Task<ActionResult<EnergyDto>> GetEnergy(
            string deviceId,
            [FromQuery] DateTime? date)
        {
            var queryDate = (date ?? DateTime.UtcNow).Date;

            try
            {
                var rec = await _energyRepo.GetDeviceEnergyUsedByIdAsync(deviceId, queryDate);
                return Ok(new EnergyDto(rec.DeviceId, rec.date, rec.energyKWh, rec.estimatedCost));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Không có dữ liệu điện năng cho {deviceId} ngày {queryDate:yyyy-MM-dd}." });
            }
        }
    }
}
