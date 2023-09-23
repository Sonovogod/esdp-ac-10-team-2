using Application;
using Infrastructure;
using Infrastructure.Persistence.DataSeeding;
using NLog;
using NLog.Web;
using WebApi.Extensions;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    string connection = "server=localhost;port=5432;database=ProjectFiveG;username=postgres;password=postgres;";

    builder.Services.AddDbConfigure(connection!);
    builder.Services.AddInfrastructureServices();
    builder.Services.AddApplicationServices();
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        using (var scope = app.Services.CreateScope())
        {
            var dataseed = scope.ServiceProvider.GetRequiredService<DataSeed>();
            await dataseed.SeedExecutiveCompany();
            await dataseed.SeedRoles();
            await dataseed.SeedAdmin();
            await dataseed.SeedAntenna();
            await dataseed.SeedTranslator();
            await dataseed.SeedContrAgents();
            await dataseed.SeedDistricts();
            await dataseed.SeedTowns();
            await dataseed.ProjectStatus();
            await dataseed.SeedRadiationZone();
            await dataseed.SeedTranslatorType();
        }
    }

    app.ConfigureCustomExceptionMiddleware();

    app.UseCors();

    app.UseHttpsRedirection();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    logger.Error(e);
}
finally
{
    LogManager.Shutdown();
}