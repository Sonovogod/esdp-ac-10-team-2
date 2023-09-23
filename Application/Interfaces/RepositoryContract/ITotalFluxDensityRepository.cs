using Application.Interfaces.RepositoryContract.Common;
using Application.Models.EnergyResult;
using Domain.Entities;

namespace Application.Interfaces.RepositoryContract;

public interface ITotalFluxDensityRepository : IBaseRepository<TotalFluxDensity>
{
    void Delete(List<TotalFluxDensity> totalFlux);
}