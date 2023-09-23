using Application.DataObjects;
using Application.Extensions;
using Application.Interfaces;
using Application.Interfaces.RepositoryContract.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class BiohazardRadiusService : IBiohazardRadiusService
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public BiohazardRadiusService(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<BaseResponse<bool>> Create(string id)
    {
        var projectAntennae = _repositoryWrapper.ProjectAntennaRepository.GetAllByCondition(x => x.ProjectId.ToString() == id).ToList();
        foreach (var projectAntenna in projectAntennae)
        {
            var antennaTranslators = _repositoryWrapper
                .AntennaTranslatorRepository.GetAllByCondition(x=>x.ProjectAntenna == projectAntenna).ToList();
            foreach (var value in antennaTranslators)
            {
                var translator = value.TranslatorSpecs;
                var radiationZonesHorizontal = _repositoryWrapper.RadiationZoneRepository
                    .GetAllByCondition(x=> x.TranslatorSpecsId == translator.Id 
                                           && x.DirectionType == DirectionType.Horizontal.GetDescription() && x.Degree != 360)
                    .OrderBy(x=>x.Degree).ToList();
                var radiationZonesVertical = _repositoryWrapper.RadiationZoneRepository
                    .GetAllByCondition(x=> x.TranslatorSpecsId == translator.Id 
                                           && x.DirectionType == DirectionType.Vertical.GetDescription() && x.Degree != 360)
                    .OrderBy(x=>x.Degree).ToList();
                await BioHazardCreate(radiationZonesHorizontal, projectAntenna, value);
                await BioHazardCreate(radiationZonesVertical, projectAntenna, value);
            }
            
        }

        await CreateSummaryBiohazardRadius(id);
        
        return new BaseResponse<bool>(
            Result: true,
            Messages: new List<string> { "Рассчеты произведены успешно успешно считан" },
            Success: true);
    }
    private decimal GetRB(decimal power,decimal height,decimal lost,decimal multiplier,decimal frequency) //Rb,m
    {
        var number = 10;
        var h = Multiplier(height);
        var l = Multiplier(-lost);
        double rB = 0;
        if (frequency <= 300)
        {
            number = 3;
            rB = Math.Sqrt(30 * (double)power * (double)h * (double)l / number) * 1 * (double)multiplier;

        }
        else if (frequency > 300 && frequency < 3000)
            rB = Math.Sqrt(8 * (double)power * (double)h * (double)l / number) * 1 * (double)multiplier;
        else if (frequency > 3000)
            rB = Math.Sqrt((double)power * 1 * (double)multiplier / (4 * number * (double)l));

        double result = Math.Round(rB, 3);
        return (decimal)result;
    }
    
    
    private decimal GetRZ(decimal degree,decimal rB) //Rz,m
    {
        double rZ = (double)rB * Math.Sin((double)-degree * Math.PI / 180);
        double result = Math.Round(rZ, 3);
        return (decimal)result;
    }
    
    private decimal GetRX(decimal degree,decimal rB) //Rx,m
    {
        double rZ = (double)rB * Math.Cos((double)-degree * Math.PI / 180);
        double result = Math.Round(rZ, 3);
        return (decimal)result;
    }
    public decimal Multiplier(decimal value) //перевод в разы
    {
        double baseNumber = 10;
        double exponent = (double)value / baseNumber;

        double result = Math.Pow(baseNumber, exponent);
        return (decimal)result;
    }

    private async Task<bool> BioHazardCreate(List<RadiationZone> radiationZones,ProjectAntenna projectAntenna, AntennaTranslator antennaTranslator )
    {
        var tilt = (int)projectAntenna.Tilt + (int)antennaTranslator.Tilt;
        var frequency = antennaTranslator.TranslatorSpecs.Frequency;
        for (int i = 0; i < radiationZones.Count; i++)
        {
            int newIndex = (i + (radiationZones.Count - tilt)) % radiationZones.Count;
            if (radiationZones[i].DirectionType == DirectionType.Horizontal.GetDescription())
                newIndex = i;
            var dbRaz = Multiplier(radiationZones[newIndex].Value);
            var maxRadius = GetRB(antennaTranslator.Power, antennaTranslator.Gain, antennaTranslator.TransmitLossFactor, dbRaz,frequency);
            BiohazardRadius biohazardRadius = new BiohazardRadius()
            {
                Degree = i,
                Db = radiationZones[newIndex].Value,
                DbRaz = dbRaz,
                MaximumBiohazardRadius = maxRadius,
                BiohazardRadiusX = GetRX(radiationZones[i].Degree, maxRadius),
                BiohazardRadiusZ = GetRZ(radiationZones[i].Degree, maxRadius),
                DirectionType = radiationZones[newIndex].DirectionType,
                AntennaTranslatorId = antennaTranslator.Id
            };
            if (i == 0)
            {
                BiohazardRadius biohazardRadiusMax = new BiohazardRadius()
                {
                    Degree = 360,
                    Db = radiationZones[newIndex].Value,
                    DbRaz = dbRaz,
                    MaximumBiohazardRadius = maxRadius,
                    BiohazardRadiusX = GetRX(radiationZones[i].Degree, maxRadius),
                    BiohazardRadiusZ = GetRZ(radiationZones[i].Degree, maxRadius),
                    DirectionType = radiationZones[newIndex].DirectionType,
                    AntennaTranslatorId = antennaTranslator.Id
                };
                await _repositoryWrapper.BiohazardRadiusRepository.CreateAsync(biohazardRadiusMax);
            }
            await _repositoryWrapper.BiohazardRadiusRepository.CreateAsync(biohazardRadius);
        }
        await _repositoryWrapper.Save();
        return true;
    }



    public async Task<BaseResponse<bool>> CreateSummaryBiohazardRadius(string id)
    {
        var projectAntennae = _repositoryWrapper.ProjectAntennaRepository.GetAllByCondition(x => x.ProjectId.ToString() == id).ToList();
        
        foreach (var projectAntenna in projectAntennae)
        {
            double[] sumVertical = new double[361];
            double[] sumHorizontal = new double[361];
            double[] sumVerticalX = new double[361];
            double[] sumHorizontalX = new double[361];
            double[] sumVerticalZ = new double[361];
            double[] sumHorizontalZ = new double[361];
            Guid antennaTranslatorId = Guid.Empty;
            var antennaTranslators = _repositoryWrapper
                .AntennaTranslatorRepository.GetAllByCondition(x=>x.ProjectAntenna == projectAntenna).ToList();
            foreach (var value in antennaTranslators)
            {
                antennaTranslatorId = value.Id;
                var bioVertical = _repositoryWrapper.BiohazardRadiusRepository.GetAllByCondition(x =>
                    x.AntennaTranslator == value && x.DirectionType == DirectionType.Vertical.GetDescription()).OrderBy(x=>x.Degree).ToList();
                var bioHorizontal = _repositoryWrapper.BiohazardRadiusRepository.GetAllByCondition(x =>
                    x.AntennaTranslator == value && x.DirectionType == DirectionType.Horizontal.GetDescription()).OrderBy(x=>x.Degree).ToList();
                for (int i = 0; i < bioVertical.Count; i++)
                {
                    sumVertical[i] += Math.Pow((double)bioVertical[i].MaximumBiohazardRadius, 2);
                    sumHorizontal[i] += Math.Pow((double)bioHorizontal[i].MaximumBiohazardRadius, 2);
                    sumVerticalX[i] += Math.Pow((double)bioVertical[i].BiohazardRadiusX, 2);
                    sumHorizontalX[i] += Math.Pow((double)bioHorizontal[i].BiohazardRadiusX, 2);
                    sumVerticalZ[i] += Math.Pow((double)bioVertical[i].BiohazardRadiusZ, 2);
                    sumHorizontalZ[i] += Math.Pow((double)bioHorizontal[i].BiohazardRadiusZ, 2);
                }
            }
            for (int i = 0; i < sumHorizontal.Length; i++)
            {   
                decimal biohazardRadiusZHorizontal = (decimal)Math.Sqrt(Math.Abs(sumHorizontalZ[i]));
                decimal biohazardRadiusZVertical = (decimal)Math.Sqrt(Math.Abs(sumVerticalZ[i]));

                if (i < 180)
                {
                    biohazardRadiusZHorizontal = -biohazardRadiusZHorizontal;
                    biohazardRadiusZVertical = -biohazardRadiusZVertical;
                }

                SummaryBiohazardRadius biohazardRadiusHorizontal = new SummaryBiohazardRadius()
                {
                    Degree = i,
                    MaximumBiohazardRadius = (decimal)Math.Sqrt(sumHorizontal[i]),
                    BiohazardRadiusX = (decimal)Math.Sqrt(sumHorizontalX[i]),
                    BiohazardRadiusZ = biohazardRadiusZHorizontal,
                    DirectionType = DirectionType.Horizontal,
                    AntennaTranslatorId = antennaTranslatorId
                };

                SummaryBiohazardRadius biohazardRadiusVertical = new SummaryBiohazardRadius()
                {
                    Degree = i,
                    MaximumBiohazardRadius = (decimal)Math.Sqrt(sumVertical[i]),
                    BiohazardRadiusX = (decimal)Math.Sqrt(sumVerticalX[i]),
                    BiohazardRadiusZ = biohazardRadiusZVertical,
                    DirectionType = DirectionType.Vertical,
                    AntennaTranslatorId = antennaTranslatorId
                };
                await _repositoryWrapper.SummaryBiohazardRadiusRepository.CreateAsync(biohazardRadiusHorizontal);
                await _repositoryWrapper.SummaryBiohazardRadiusRepository.CreateAsync(biohazardRadiusVertical);
            }
        }
        await _repositoryWrapper.Save();
        return new BaseResponse<bool>(
            Result: true,
            Messages: new List<string> { "Рассчеты произведены успешно успешно считан" },
            Success: true);
    }
}