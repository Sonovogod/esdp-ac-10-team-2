using Application.DataObjects;
using Application.Interfaces;
using Application.Interfaces.RepositoryContract.Common;
using Application.Models.EnergyResult;
using Application.Validation;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class TotalFluxDensityService : ITotalFluxDensityService
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly TotalFluxDensityResultValidator _totalFluxDensityResultValidator;
    private readonly int[] _distances;
    
    public TotalFluxDensityService(TotalFluxDensityResultValidator totalFluxDensityResultValidator, IMapper mapper, IRepositoryWrapper repositoryWrapper)
    {
        _totalFluxDensityResultValidator = totalFluxDensityResultValidator;
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
        _distances = new []{5, 10, 20, 30, 40, 60, 80, 100};
    }
    
    public async Task<BaseResponse<string>> CreateAsync(List<EnergyResult> energyResults,string projectId, string creator)
    {
        var createTotalFluxDensity = PowerDensitySummation(energyResults,projectId);
        List<TotalFluxDensity> totalFluxDensities = _mapper.Map<List<TotalFluxDensity>>(createTotalFluxDensity);
        foreach (var totalFlux in totalFluxDensities)
        {
            var validationResult = await _totalFluxDensityResultValidator.ValidateAsync(totalFlux);
            if (!validationResult.IsValid)
            {
                List<string> messages = _mapper.Map<List<string>>(validationResult.Errors);
                return new BaseResponse<string>(
                    Result: "", 
                    Messages: messages,
                    Success: false);
            }
            totalFlux.CreatedBy = creator;
            await _repositoryWrapper.TotalFluxDensityRepository.CreateAsync(totalFlux);
        }
        await _repositoryWrapper.Save();
        return new BaseResponse<string>(
            Result: "",
            Success: true,
            Messages: new List<string>{"Просчеты суммарной плотности потока энергии успешно созданы"});
    }

    public BaseResponse<List<TotalFluxDensityDto>> GetAllByOid(string id)
    {
        var totalFluxDensities =  _repositoryWrapper.TotalFluxDensityRepository.GetAllByCondition
            (x => x.ProjectId.ToString() == id).OrderBy(x=>x.Distance);
        List<TotalFluxDensityDto> model = _mapper.Map<List<TotalFluxDensityDto>>(totalFluxDensities);
        if (totalFluxDensities is null)
            return new BaseResponse<List<TotalFluxDensityDto>>(
                Result: null,
                Messages: new List<string>{"Просчеты плотности потока энергии не найдены"},
                Success: true);
        return new BaseResponse<List<TotalFluxDensityDto>>(
            Result: model,
            Success: true,
            Messages: new List<string>{"Просчеты плотности потока энергии успешно найдены"});
    }

    public async Task<BaseResponse<bool>> Delete(string id)
    {
        TotalFluxDensity? totalFluxDensity = await _repositoryWrapper.TotalFluxDensityRepository.GetByCondition(x => x.Id.ToString() == id);
        if (totalFluxDensity is not null)
        {
            totalFluxDensity.IsDelete = true;
            _repositoryWrapper.TotalFluxDensityRepository.Update(totalFluxDensity);
            await _repositoryWrapper.Save();

            return new BaseResponse<bool>(
                Result: true,
                Success: true,
                Messages: new List<string>{"Просчет суммарной плотности потока энергии успешно удален"});
        }
        return new BaseResponse<bool>(
            Result: false, 
            Messages: new List<string>{"Просчет суммарной плотности потока энергии не существует"},
            Success: false);
    }

    private List<CreateTotalFluxDensityDto> PowerDensitySummation(List<EnergyResult> energyResults,string projectId)
    {
        List<CreateTotalFluxDensityDto> totalFluxDensities = new List<CreateTotalFluxDensityDto>();

        foreach (var distance in _distances)
        {
            var totalFluxDensity = new CreateTotalFluxDensityDto { Distance = distance,ProjectId = new Guid(projectId)};
            foreach (var energyResult in energyResults)
            {
                if (energyResult.Distance == distance)
                {
                    totalFluxDensity.Value += energyResult.Value;
                }
            }
            totalFluxDensities.Add(totalFluxDensity);
        }

        return totalFluxDensities;
    }
}