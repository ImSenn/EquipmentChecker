using CheckerBA.Application.DTOs;
using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;
using CheckerBA.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CheckerBA.Application.Services
{
    public class TelemetryProcessingService
    {
        private readonly ITelemetryRepository _telemetryRepo;
        private readonly IEnergyUsedRepository _energyRepo;
        private readonly IHubContext<DeviceHub> _hubContext;
        private const double TelemetryIntervalSeconds = 5.0;
        private const double CostPerKWh = 3000;

        public TelemetryProcessingService(
            ITelemetryRepository telemetryRepo,
            IEnergyUsedRepository energyRepo,
            IHubContext<DeviceHub> hubContext)
        {
            _telemetryRepo = telemetryRepo;
            _energyRepo = energyRepo;
            _hubContext = hubContext;
        }

        public async Task ProcessTelemetryAsync(Telemetry telemetry)
        {
            // 1. Lưu vào MongoDB
            await _telemetryRepo.AddTelemetryAsync(telemetry);

            // 2. Tính & cập nhật điện năng
            double kWh = (telemetry.Metrics.estimatedPower / 1000.0) * (TelemetryIntervalSeconds / 3600.0);
            var today = telemetry.Timestamp.Date;

            try
            {
                var rec = await _energyRepo.GetDeviceEnergyUsedByIdAsync(telemetry.DeviceId, today);
                rec.energyKWh += kWh;
                rec.estimatedCost = rec.energyKWh * CostPerKWh;
                await _energyRepo.UpsertEnergyUsedAsync(rec);
            }
            catch (KeyNotFoundException)
            {
                await _energyRepo.UpsertEnergyUsedAsync(new EnergyUsed
                {
                    DeviceId = telemetry.DeviceId,
                    date = today,
                    energyKWh = kWh,
                    estimatedCost = kWh * CostPerKWh
                });
            }

            // 3. Broadcast qua SignalR tới group của thiết bị
            var dto = new TelemetryDto(
                telemetry.DeviceId,
                telemetry.Timestamp,
                new MetricsDto(telemetry.Metrics.Temperature, telemetry.Metrics.Vibration, telemetry.Metrics.estimatedPower),
                new StatusDto(telemetry.Status.runState, telemetry.Status.powerState),
                new SystemDto(telemetry.System.upTime, telemetry.System.wifiRssi, telemetry.System.freeHeap));

            await _hubContext.Clients.Group(telemetry.DeviceId)
                .SendAsync("ReceiveTelemetry", dto);
        }

        public Task<List<Telemetry>> GetTelemetryHistoryAsync(string deviceId, DateTime startTime, DateTime endTime)
            => _telemetryRepo.GetTelemetryHistoryAsync(deviceId, startTime, endTime);
    }
}