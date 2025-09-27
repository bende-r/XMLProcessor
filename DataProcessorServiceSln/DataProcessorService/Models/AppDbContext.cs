using Microsoft.EntityFrameworkCore;

namespace DataProcessorService.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ModuleStatus> ModuleStatuses { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}