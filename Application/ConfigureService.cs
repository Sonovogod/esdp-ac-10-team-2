using System.Reflection;
using Application.Common;
using Application.Interfaces;
using Application.Services;
using Application.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Application;

public static class ConfigureService
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IServiceWrapper, ServiceWrapper>();
        services.AddScoped<IEnergyFlowService, EnergyFlowService>();
        services.AddScoped<ITotalFluxDensityService, TotalFluxDensityService>();
        services.AddScoped<IBiohazardRadiusService, BiohazardRadiusService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRadiationZoneService, RadiationZoneService>();
        services.AddScoped<UserValidator>();
        services.AddScoped<ProjectValidator>();
        services.AddScoped<UpdateProjectValidator>();
        services.AddScoped<AntennaTranslatorValidator>();
        services.AddScoped<ContrAgentValidator>();
        services.AddScoped<AntennaValidator>();
        services.AddScoped<TranslatorSpecsValidator>();
        services.AddScoped<EnergyResultValidator>();
        services.AddScoped<TotalFluxDensityResultValidator>();
        services.AddScoped<RoleValidator>();
        services.AddScoped<RadiationZoneValidator>();
        services.AddScoped<ExecutiveCompanyValidator>();
        services.AddScoped<ProjectAntennaValidator>();
        services.AddScoped<TranslatorTypeValidator>();
        services.AddScoped<UpdateUserValidator>();
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = AuthenticationOptions.ISSUER,
                    ValidAudience = AuthenticationOptions.AUDIENCE,
                    IssuerSigningKey = AuthenticationOptions.GetSymmetricSecurityKey()
                };
            });
        
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });
        
        return services;
    }
}