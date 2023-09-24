using Application;
using Infrastructure;
using Infrastructure.Persistence.DataContext;
using Infrastructure.Persistence.DataSeeding;
using Microsoft.EntityFrameworkCore;
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

    //string connection = "Server=localhost;port=5432;database=Project5G;username=postgres;password=123;";
    string connection = "host=host.docker.internal;port=5432;database=Test5GProject;username=postgres;password=123;";

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
    //if (app.Environment.IsDevelopment())
    //{
        app.UseSwagger();
        app.UseSwaggerUI();
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Project5GDbContext>();
            await context.Database.MigrateAsync();
        }
    //}

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