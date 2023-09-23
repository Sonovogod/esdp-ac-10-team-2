using Application.DataObjects;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.RepositoryContract.Common;
using Application.Models.EnergyResult;
using Application.Validation;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

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

    public List<CreateTotalFluxDensityDto> PowerDensitySummation(List<EnergyResult> energyResults)
    {
        List<CreateTotalFluxDensityDto> totalFluxDensities = new List<CreateTotalFluxDensityDto>();

        foreach (var distance in _distances)
        {
            var totalFluxDensity = new CreateTotalFluxDensityDto { Distance = distance };
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

    private async Task<List<EnergyResult>> PowerDensity(CreateEnergyResultDto inputData)
    {
        List<EnergyResult> energyResults = new List<EnergyResult>();
        decimal gainInMultiplier = Multiplier(inputData.Gain);
        decimal transmitLossFactorInMultiplier = Multiplier(-inputData.TransmitLossFactor);
        decimal heightInstall = inputData.HeightInstall;
        decimal powerSignal = inputData.PowerSignal;
        AntennaTranslator? antennaTranslator =
           await _repositoryWrapper.AntennaTranslatorRepository.GetByCondition(x => x.Id == inputData.AntennaTranslatorId);
        var translatorId = antennaTranslator.TranslatorSpecsId;
        
        foreach (var distance in _distances)
        {
            decimal normalizedVerticalPowerResult = (decimal)Math.Pow((double)NormalizedVerticalPower(distance, heightInstall, translatorId), 2);
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

    private decimal NormalizedVerticalPower(decimal distance, decimal heightInstall, Guid translatorId) //F(θ)
    {
        int degree = (int)Math.Ceiling(Math.Atan((double)((heightInstall - HumanHeight) / distance)) * 180 / Math.PI);
        var radiationZones = _repositoryWrapper.RadiationZoneRepository.GetAllByCondition(x => x.TranslatorSpecsId == translatorId).Where(x=> x.DirectionType == DirectionType.Vertical.GetDescription());
        RadiationZone? verticalRadiation = radiationZones.FirstOrDefault(x => x.Degree == degree);
        var verticalRadiationInMultiplier = Multiplier(verticalRadiation.Value);
        
        return verticalRadiationInMultiplier;
    }

    public async Task<BaseResponse<string>> CreateAsync(string projectId, string creator)
    {
        List<CreateEnergyResultDto> createEnergyResultsDto = new List<CreateEnergyResultDto>();
        
        var projectAntennas =
             _repositoryWrapper.ProjectAntennaRepository.GetAllByCondition(
                x => x.ProjectId.ToString() == projectId).ToList();

        if (projectAntennas.Count != 0)
        {
            foreach (var projectAntenna in projectAntennas)
            {
                var antennaTranslators =
                    _repositoryWrapper.AntennaTranslatorRepository.GetAllByCondition(x =>
                        x.ProjectAntennaId == projectAntenna.Id).ToList();
                
                if (antennaTranslators.Count == 0) break;
                foreach (var antennaTranslator in antennaTranslators)
                {
                    //вызвать метод очистки предыдущих значений или предусмотреть удаление значений после просчетов
                    CreateEnergyResultDto createEnergyResultDto = new CreateEnergyResultDto
                    {
                        PowerSignal = antennaTranslator.Power,
                        Gain = antennaTranslator.Gain,
                        TransmitLossFactor = antennaTranslator.TransmitLossFactor,
                        HeightInstall = projectAntenna.HeightFromGroundLevel,
                        AntennaTranslatorId = antennaTranslator.Id
                    };
                    createEnergyResultsDto.Add(createEnergyResultDto);
                }
            }
        }

        if (createEnergyResultsDto.Count == 0)
        {
            return new BaseResponse<string>(
                Result: "",
                Messages: new List<string> {"Не удалось выполнить просчеты проверьте проектные антенны и их передатчики"},
                Success: false);
        }

        foreach (var energyResult in createEnergyResultsDto)
        {
            var validationResult = await _energyResultValidator.ValidateAsync(energyResult);
            if (!validationResult.IsValid)
            {
                List<string> messages = _mapper.Map<List<string>>(validationResult.Errors);
                return new BaseResponse<string>(
                    Result: "", 
                    Messages: messages,
                    Success: false);
            }
            
            List<EnergyResult> calculationResults = await PowerDensity(energyResult);
        
            foreach (var calculationResult in calculationResults)
            {
                calculationResult.CreatedBy = creator;
                await _repositoryWrapper.EnergyFlowRepository.CreateAsync(calculationResult);
            }
        }
            
        
        await _repositoryWrapper.Save();
        return new BaseResponse<string>(
            Result: "",
            Success: true,
            Messages: new List<string>{"Просчеты плотности потока энергии успешно созданы"});
    }

    public BaseResponse<List<EnergyResultDto>> GetAllByOid(string oid)
    {
        var energyResults =  _repositoryWrapper.EnergyFlowRepository.GetAllByCondition
            (x => x.AntennaTranslatorId.ToString() == oid);
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

    public async Task<BaseResponse<bool>> Delete(string id)
    {
        EnergyResult? energyResult = await _repositoryWrapper.EnergyFlowRepository.GetByCondition(x => x.Id.ToString() == id);
        if (energyResult is not null)
        {
            energyResult.IsDelete = true;
            _repositoryWrapper.EnergyFlowRepository.Update(energyResult);
            await _repositoryWrapper.Save();

            return new BaseResponse<bool>(
                Result: true,
                Success: true,
                Messages: new List<string>{"Просчет плотности потока энергии успешно удален"});
        }
        return new BaseResponse<bool>(
            Result: false, 
            Messages: new List<string>{"Просчет плотности потока энергии не существует"},
            Success: false);
    }
    
    public decimal Multiplier(decimal value) //перевод в разы
    {
        return (decimal)Math.Pow(10, (double)value / 10);
    }
}