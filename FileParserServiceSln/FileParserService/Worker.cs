using FileParserService.Services.Interfaces;

namespace FileParserService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _watchPath;
        private readonly string _fileFilter;
        private readonly string _processingMode;
        private readonly Dictionary<string, DateTime> _processedFiles = new();

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _watchPath = configuration["FileWatcher:Path"]!;
            _fileFilter = configuration["FileWatcher:Filter"]!;

            _processingMode = configuration["FileWatcher:ProcessingMode"] ?? "DeleteAfterProcessing";
            _logger.LogInformation("File processing mode is set to: {ProcessingMode}", _processingMode);

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
                    List<string> filesToProcess = new List<string>();

                    if (_processingMode == "TrackLastModified")
                    {
                        var currentFiles = Directory.GetFiles(_watchPath, _fileFilter);
                        foreach (var file in currentFiles)
                        {
                            var lastWriteTime = File.GetLastWriteTimeUtc(file);

                            if (!_processedFiles.TryGetValue(file, out var processedTime) || lastWriteTime > processedTime)
                            {
                                filesToProcess.Add(file);
                            }
                        }
                    }
                    else
                    {
                        filesToProcess = Directory.GetFiles(_watchPath, _fileFilter).ToList();
                    }

                    if (filesToProcess.Any())
                    {
                        _logger.LogInformation("Found {Count} file(s) to process in '{ProcessingMode}' mode.", filesToProcess.Count, _processingMode);

                        foreach (var file in filesToProcess)
                        {
                            if (_processingMode == "TrackLastModified")
                            {
                                _processedFiles[file] = File.GetLastWriteTimeUtc(file);
                            }

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