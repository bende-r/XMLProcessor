namespace FileParserService.Services.Interfaces
{
    public interface IFileProcessorService
    {
        Task ProcessFileAsync(string filePath);
    }
}
