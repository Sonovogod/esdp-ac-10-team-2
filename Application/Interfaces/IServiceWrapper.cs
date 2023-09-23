namespace Application.Interfaces;

public interface IServiceWrapper
{
    IFileService FileService { get; }
    IUserService UserService { get; }
    ITokenService TokenService { get; }
    IProjectService ProjectService { get; }
    IAuthorizationService AuthorizationService { get; }
    IContrAgentService ContrAgentService { get; }
    IDistrictService DistrictService { get; }
    ITownService TownService { get; }
    IAntennaService AntennaService { get; }
    ITranslatorSpecsService TranslatorSpecsService { get; }
    IProjectAntennaService ProjectAntennaService { get; }
    IEnergyFlowService EnergyFlowService { get; }
    ITotalFluxDensityService TotalFluxDensityService { get; }
    IRoleService RoleService { get; }
    IRadiationZoneService RadiationZoneService { get; }
    IRadiationZoneExelFileService RadiationZoneExelFileService{ get; }
    IExecutiveCompanyService ExecutiveCompanyService { get; }
    IAntennaTranslatorService AntennaTranslatorService { get; }
    ITranslatorTypeService TranslatorTypeService { get; }
    IBiohazardRadiusService BiohazardRadiusService { get; }
    IExportProjectService ExportProjectService { get; }
    IProjectImageService ProjectImageService { get; }
}