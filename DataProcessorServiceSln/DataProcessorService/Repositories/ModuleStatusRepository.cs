using DataProcessorService.Data;
using DataProcessorService.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DataProcessorService.Repositories
{
    public class ModuleStatusRepository : IModuleStatusRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ModuleStatusRepository> _logger;

        public ModuleStatusRepository(AppDbContext context, ILogger<ModuleStatusRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpsertModuleStatusAsync(ModuleStatus newStatus)
        {
            var existingStatus = await _context.ModuleStatuses
                .FirstOrDefaultAsync(m => m.ModuleCategoryID == newStatus.ModuleCategoryID);

            if (existingStatus != null)
            {
                existingStatus.ModuleState = newStatus.ModuleState;
                existingStatus.LastUpdate = DateTime.UtcNow;
                _context.ModuleStatuses.Update(existingStatus);
                _logger.LogInformation("Updating Module '{ModuleID}' with state '{State}'.", newStatus.ModuleCategoryID, newStatus.ModuleState);
            }
            else
            {
                newStatus.LastUpdate = DateTime.UtcNow;
                await _context.ModuleStatuses.AddAsync(newStatus);
                _logger.LogInformation("Inserting new Module '{ModuleID}' with state '{State}'.", newStatus.ModuleCategoryID, newStatus.ModuleState);
            }

            await _context.SaveChangesAsync();
        }
    }
}