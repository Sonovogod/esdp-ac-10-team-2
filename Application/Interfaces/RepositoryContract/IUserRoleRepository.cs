using Application.Interfaces.RepositoryContract.Common;
using Domain.Entities;

namespace Application.Interfaces.RepositoryContract;

public interface IUserRoleRepository : IBaseRepository<UserRole>
{
    void Delete(UserRole userRole);
}