using Application.DataObjects;
using Application.Interfaces;
using Application.Interfaces.RepositoryContract.Common;
using Application.Models.EnergyResult;
using Application.Validation;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class EnergyFlowService : IEnergyFlowService
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly EnergyResultValidator _energyResultValidator;
    private const int GroundInfluenceFactor = 1;
    private const int HumanHeight = 2;
    private readonly int[] _distances;

    public EnergyFlowService(EnergyResultValidator energyResultValidator, IMapper mapper, IRepositoryWrapper repositoryWrapper)
    {
        _energyResultValidator = energyResultValidator;
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
        _distances = new []{5, 10, 20, 30, 40, 60, 80, 100};
    }

    public List<TotalFluxDensity> PowerDensitySummation(EnergyResult energyResult)
    {
        throw new NotImplementedException();
    }

    private List<EnergyResult> PowerDensity(CreateEnergyResultDto inputData)
    {
        List<EnergyResult> energyResults = new List<EnergyResult>();
        decimal gainInMultiplier = Multiplier(inputData.Gain);
        decimal transmitLossFactorInMultiplier = Multiplier(inputData.TransmitLossFactor);
        decimal heightInstall = inputData.HeightInstall;
        decimal powerSignal = inputData.PowerSignal;
        
        foreach (var distance in _distances)
        {
            decimal normalizedVerticalPowerResult = (decimal)Math.Pow((double)NormalizedVerticalPower(), 2);
            decimal euclideanDistanceResult = (decimal)Math.Pow((double)EuclideanDistance(heightInstall, distance), 2);
            
            var result = 8 * powerSignal * gainInMultiplier * GroundInfluenceFactor * transmitLossFactorInMultiplier *
                normalizedVerticalPowerResult / euclideanDistanceResult;
            
            EnergyResult energyResult = new EnergyResult
            {
                Distance = distance,
                Value = result,
                AntennaTranslatorId = inputData.AntennaTranslatorId
            };
            energyResults.Add(energyResult);
        }

        return energyResults;
    }

    private decimal EuclideanDistance(decimal heightInstall, int distance) //R,m
    {
        var result = (decimal)Math.Sqrt(Math.Pow((double)(heightInstall - HumanHeight), 2) + Math.Pow(distance, 2));
        return result;
    }

    private decimal NormalizedVerticalPower() //F(θ)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResponse<string>> CreateAsync(CreateEnergyResultDto createEnergyResultDto, string creator)
    {
        var validationResult = await _energyResultValidator.ValidateAsync(createEnergyResultDto);
        if (validationResult.IsValid)
        {
            List<EnergyResult> calculationResults = PowerDensity(createEnergyResultDto);
            
            foreach (var calculationResult in calculationResults)
            {
                calculationResult.CreatedBy = creator;
                await _repositoryWrapper.EnergyFlowRepository.CreateAsync(calculationResult);
            }

            await _repositoryWrapper.Save();
            return new BaseResponse<string>(
                Result: "",
                Success: true,
                Messages: new List<string>{"Просчеты плотности потока энергии успешно созданы"});
        }
        
        List<string> messages = _mapper.Map<List<string>>(validationResult.Errors);
        return new BaseResponse<string>(
            Result: "", 
            Messages: messages,
            Success: false);
    }

    public BaseResponse<List<EnergyResultDto>> GetAllByOid(string oid)
    {
        var energyResults =  _repositoryWrapper.EnergyFlowRepository.GetAllByCondition(x => x.AntennaTranslatorId.ToString() == oid);
        List<EnergyResultDto> model = _mapper.Map<List<EnergyResultDto>>(energyResults);
        if (energyResults is null)
            return new BaseResponse<List<EnergyResultDto>>(
                Result: null,
                Messages: new List<string>{"Просчеты плотности потока энергии не найдены"},
                Success: true);
        return new BaseResponse<List<EnergyResultDto>>(
            Result: model,
            Success: true,
            Messages: new List<string>{"Просчеты плотности потока энергии успешно найдены"});
    }

    public async Task<BaseResponse<bool>> Delete(List<EnergyResult> energyResults)
    {
        if (energyResults.Count > 0)
        {
            foreach (var energyResult in energyResults)
            {
                energyResult.IsDelete = true;
                _repositoryWrapper.EnergyFlowRepository.Update(energyResult);
            }
            
            await _repositoryWrapper.Save();

            return new BaseResponse<bool>(
                Result: true,
                Success: true,
                Messages: new List<string>{"Просчеты плотности потока энергии успешно удалены"});
        }
        return new BaseResponse<bool>(
            Result: false, 
            Messages: new List<string>{"Просчеты плотности потока энергии не существуют"},
            Success: false);
    }
    
    private decimal Multiplier(decimal value) //перевод в разы
    {
        return (decimal)Math.Pow((double)value / 10, 10);
    }
}