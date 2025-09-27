using FileParserService.Services.Interfaces;

namespace FileParserService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _watchPath;
        private readonly string _fileFilter;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _watchPath = configuration["FileWatcher:Path"]!;
            _fileFilter = configuration["FileWatcher:Filter"]!;

            if (!Directory.Exists(_watchPath))
            {
                _logger.LogWarning("Watch directory '{WatchPath}' does not exist. Creating it.", _watchPath);
                Directory.CreateDirectory(_watchPath);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FileParserService started. Watching directory: {Path}", _watchPath);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var files = Directory.GetFiles(_watchPath, _fileFilter);
                    if (files.Any())
                    {
                        _logger.LogInformation("Found {Count} file(s) to process.", files.Length);

                        foreach (var file in files)
                        {
                            _ = Task.Run(async () =>
{
    using var scope = _scopeFactory.CreateScope();
    var fileProcessor = scope.ServiceProvider.GetRequiredService<IFileProcessorService>();
    await fileProcessor.ProcessFileAsync(file);
}, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the monitoring loop.");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}