using Application.Interfaces.RepositoryContract.Common;
using Domain.Entities;

namespace Application.Interfaces.RepositoryContract;

public interface IBiohazardRadiusRepository : IBaseRepository<BiohazardRadius>
{
    void Delete(List<BiohazardRadius> biohazardRadii);
}