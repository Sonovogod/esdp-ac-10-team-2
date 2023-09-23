using Application.Interfaces.RepositoryContract;
using Domain.Entities;
using Infrastructure.Persistence.DataContext;
using Infrastructure.Repositories;

namespace Infrastructure.Services;

public class BiohazardRadiusRepository : BaseRepository<BiohazardRadius>,IBiohazardRadiusRepository
{
    public BiohazardRadiusRepository(Project5GDbContext dbContext) : base(dbContext)
    {
    }

    public void Delete(List<BiohazardRadius> biohazardRadii)
    {
        DbContext.BiohazardRadii.RemoveRange(biohazardRadii);
    }
}