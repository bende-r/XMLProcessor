using System.ComponentModel.DataAnnotations;

namespace DataProcessorService.Data
{
    public class ModuleStatus
    {
        [Key]
        public string ModuleCategoryID { get; set; } = string.Empty;

        [Required]
        public string? ModuleState { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
