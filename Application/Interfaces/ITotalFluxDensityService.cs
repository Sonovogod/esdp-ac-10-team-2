using Application.DataObjects;
using Application.Models.EnergyResult;
using Domain.Entities;

namespace Application.Interfaces;

public interface ITotalFluxDensityService
{
    Task<BaseResponse<string>> CreateAsync(List<EnergyResult> energyResults,string projectId, string creator);
    BaseResponse<List<TotalFluxDensityDto>> GetAllByOid(string id);
    Task<BaseResponse<bool>> Delete(string id);
}