using Application.Interfaces.RepositoryContract.Common;
using Domain.Entities;

namespace Application.Interfaces.RepositoryContract;

public interface ISummaryBiohazardRadiusRepository : IBaseRepository<SummaryBiohazardRadius>
{
    void Delete(List<SummaryBiohazardRadius> summaryBiohazardRadii);
}