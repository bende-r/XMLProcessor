using FileParserService;
using FileParserService.Services;
using FileParserService.Services.Interfaces;

using MassTransit;

using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger());

var services = builder.Services;

services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbitMqConfig["HostName"], (ushort)rabbitMqConfig.GetValue<int>("Port"), "/", h =>
        {
            h.Username(rabbitMqConfig["UserName"]);
            h.Password(rabbitMqConfig["Password"]);
        });
    });
});

services.AddSingleton(builder.Configuration);

services.AddScoped<IFileProcessorService, FileProcessorService>();
services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();