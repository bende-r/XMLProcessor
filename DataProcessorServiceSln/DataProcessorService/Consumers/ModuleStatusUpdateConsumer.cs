using DataProcessorService.Repositories.Interfaces;

using MassTransit;

using SharedContracts;

namespace DataProcessorService.Consumers
{
    public class ModuleStatusUpdateConsumer : IConsumer<ModuleStatusUpdate>
    {
        private readonly ILogger<ModuleStatusUpdateConsumer> _logger;
        private readonly IModuleStatusRepository _repository;

        public ModuleStatusUpdateConsumer(ILogger<ModuleStatusUpdateConsumer> logger, IModuleStatusRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<ModuleStatusUpdate> context)
        {
            _logger.LogInformation("Received {Count} module updates.", context.Message.Modules.Count);

            foreach (var module in context.Message.Modules)
            {
                if (!string.IsNullOrEmpty(module.ModuleCategoryID))
                {
                    await _repository.UpsertModuleStatusAsync(new Data.ModuleStatus
                    {
                        ModuleCategoryID = module.ModuleCategoryID,
                        ModuleState = module.ModuleState
                    });
                }
            }
        }
    }
}