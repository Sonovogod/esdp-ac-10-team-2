using Application.Interfaces.RepositoryContract.Common;
using Application.Models.EnergyResult;
using Domain.Entities;

namespace Application.Interfaces.RepositoryContract;

public interface IEnergyFlowRepository : IBaseRepository<EnergyResult>
{
    void Delete(List<EnergyResult> energyResult);
}