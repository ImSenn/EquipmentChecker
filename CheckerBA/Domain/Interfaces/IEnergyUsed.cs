using System.Collections.Generic;
using System.Threading.Tasks;
using CheckerBA.Domain.Entities;

namespace CheckerBA.Domain.Interfaces
{
    public interface IEnergyUsedRepository
    {
        Task UpsertEnergyUsedAsync(EnergyUsed energyUsed);
        Task<EnergyUsed> GetDeviceEnergyUsedByIdAsync(string deviceId, DateTime date);
    }
}