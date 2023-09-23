using Application.Interfaces.RepositoryContract;
using Application.Models.EnergyResult;
using Domain.Entities;
using Infrastructure.Persistence.DataContext;
using Infrastructure.Repositories;

namespace Infrastructure.Services;

public class EnergyFlowRepository : BaseRepository<EnergyResult>, IEnergyFlowRepository
{
    public EnergyFlowRepository(Project5GDbContext dbContext) : base(dbContext)
    {
        
    }

    public void Delete(List<EnergyResult> energyResult)
    {
        DbContext.EnergyResults.RemoveRange(energyResult);
    }
}