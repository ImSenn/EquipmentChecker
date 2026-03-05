using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;

namespace CheckerBA.Application.Services
{
    public class TelemetryProcessingService
    {
        private readonly ITelemetryRepository _telemetryRepo;
        private readonly IEnergyUsedRepository _energyRepo;

        public TelemetryProcessingService(ITelemetryRepository telemetryRepo, IEnergyUsedRepository energyRepo)
        {
            _telemetryRepo = telemetryRepo;
            _energyRepo = energyRepo;
        }

        public async Task ProcessTelemetryAsync(Telemetry telemetry)
        {
            await _telemetryRepo.AddTelemetryAsync(telemetry);

            double hoursPassed = 5.0 / 3600.0;
            double energyConsumedKWh = (telemetry.Metrics.estimatedPower / 1000.0) * hoursPassed;
            double costPerKWh = 3000;

            var today = telemetry.Timestamp.Date;

            try
            {
                // Thử tìm hóa đơn điện của ngày hôm nay xem có chưa
                var energyRecord = await _energyRepo.GetDeviceEnergyUsedByIdAsync(telemetry.DeviceId, today);

                // Nếu có rồi thì cộng dồn số điện và tiền vào
                energyRecord.energyKWh += energyConsumedKWh;
                energyRecord.estimatedCost = energyRecord.energyKWh * costPerKWh;

                await _energyRepo.UpsertEnergyUsedAsync(energyRecord);
            }
            catch (KeyNotFoundException)
            {
                var newEnergy = new EnergyUsed
                {
                    DeviceId = telemetry.DeviceId,
                    date = today,
                    energyKWh = energyConsumedKWh,
                    estimatedCost = energyConsumedKWh * costPerKWh
                };

                await _energyRepo.UpsertEnergyUsedAsync(newEnergy);
            }
        }

        public async Task<List<Telemetry>> GetTelemetryHistoryAsync(string deviceId, DateTime startTime, DateTime endTime)
        {
            return await _telemetryRepo.GetTelemetryHistoryAsync(deviceId, startTime, endTime);
        }
    }
}