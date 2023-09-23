using Application.DataObjects;
using Domain.Entities;

namespace Application.Interfaces;

public interface IBiohazardRadiusService
{
    public Task<BaseResponse<bool>> Create(string id);
    public decimal Multiplier(decimal value);
}