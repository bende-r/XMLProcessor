using MassTransit;

namespace DataProcessorService.Extensions
{
    public static class MassTransitSetup
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(MassTransitSetup).Assembly);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConfig = configuration.GetSection("RabbitMQ");
                    cfg.Host(rabbitMqConfig["HostName"], (ushort)rabbitMqConfig.GetValue<int>("Port"), "/", h =>
                    {
                        h.Username(rabbitMqConfig["UserName"]);
                        h.Password(rabbitMqConfig["Password"]);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}