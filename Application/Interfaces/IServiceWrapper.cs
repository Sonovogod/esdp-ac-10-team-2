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
    IEnergyFlowService EnergyFlowService { get; }
    IRoleService RoleService { get; }
    ICompanyLicenseService CompanyLicenseService { get; }
    IExecutiveCompanyService ExecutiveCompanyService { get; }
}