using System.Xml.Serialization;

using FileParserService.Models;
using FileParserService.Services.Interfaces;

using MassTransit;

using SharedContracts;
namespace FileParserService.Services
{
    public class FileProcessorService : IFileProcessorService
    {
        private readonly ILogger<FileProcessorService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly string _errorPath;
        private readonly Random _random = new();
        private readonly string[] _states = { "Online", "Run", "NotReady", "Offline" };

        public FileProcessorService(
            ILogger<FileProcessorService> logger,
            IPublishEndpoint publishEndpoint,
            IConfiguration configuration)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;

            _errorPath = configuration["FileWatcher:ErrorPath"]
    ?? Path.Combine(configuration["FileWatcher:Path"]!, "_error");

            if (!Directory.Exists(_errorPath))
            {
                Directory.CreateDirectory(_errorPath);
            }
        }

        public async Task ProcessFileAsync(string filePath)
        {
            _logger.LogInformation("Processing file: {FilePath}", filePath);
            try
            {
                string content = await File.ReadAllTextAsync(filePath);
                var serializer = new XmlSerializer(typeof(InstrumentStatus));
                using var reader = new StringReader(content);
                var instrumentStatus = (InstrumentStatus?)serializer.Deserialize(reader);

                if (instrumentStatus?.DeviceStatuses is null || !instrumentStatus.DeviceStatuses.Any())
                {
                    _logger.LogWarning("File {FilePath} is empty or contains no devices.", filePath);
                    return;
                }

                var moduleInfos = instrumentStatus.DeviceStatuses.Select(device => new ModuleInfo
                {
                    ModuleCategoryID = device.ModuleCategoryID,
                    ModuleState = _states[_random.Next(_states.Length)]
                }).ToList();

                var message = new ModuleStatusUpdate { Modules = moduleInfos };

                await _publishEndpoint.Publish(message);

                _logger.LogInformation("Successfully processed and published data for {Count} modules from file: {FilePath}", message.Modules.Count, filePath);

                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing file {FilePath}. Moving to error directory.", filePath);
                try
                {
                    var destinationPath = Path.Combine(_errorPath, Path.GetFileName(filePath));
                    if (File.Exists(destinationPath))
                    {
                        destinationPath = Path.Combine(_errorPath, $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(filePath)}");
                    }
                    File.Move(filePath, destinationPath);
                    _logger.LogInformation("File {FileName} moved to error directory.", Path.GetFileName(filePath));
                }
                catch (Exception moveEx)
                {
                    _logger.LogError(moveEx, "Failed to move corrupted file {FileName} to error directory.", Path.GetFileName(filePath));
                }
            }
        }
    }
}