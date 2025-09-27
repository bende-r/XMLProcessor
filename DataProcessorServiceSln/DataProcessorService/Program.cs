using DataProcessorService.Data;
using DataProcessorService.Extensions;
using DataProcessorService.Repositories;
using DataProcessorService.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Logging.AddSerilog(new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger());

var services = builder.Services;
var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection");
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

services.AddScoped<IModuleStatusRepository, ModuleStatusRepository>();

services.AddMassTransitWithRabbitMq(configuration);

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var servicesProvider = scope.ServiceProvider;
    try
    {
        var db = servicesProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var loggerFactory = servicesProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DatabaseMigration");
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

host.Run();