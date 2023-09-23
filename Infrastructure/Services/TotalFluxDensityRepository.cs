using Application.Interfaces.RepositoryContract;
using Application.Models.EnergyResult;
using Domain.Entities;
using Infrastructure.Persistence.DataContext;
using Infrastructure.Repositories;

namespace Infrastructure.Services;

public class TotalFluxDensityRepository : BaseRepository<TotalFluxDensity>, ITotalFluxDensityRepository
{
    public TotalFluxDensityRepository(Project5GDbContext dbContext) : base(dbContext)
    {
    }

    public void Delete(List<TotalFluxDensity> totalFlux)
    {
        DbContext.TotalFluxDensities.RemoveRange(totalFlux);
    }
}