using DataProcessorService.Data;

namespace DataProcessorService.Repositories.Interfaces
{
    public interface IModuleStatusRepository
    {
        Task UpsertModuleStatusAsync(ModuleStatus status);
    }
}
