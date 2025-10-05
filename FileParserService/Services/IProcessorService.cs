namespace FileParserService.Services;

internal interface IProcessorService
{
    Task ProcessFileAsync(string inputFilePath);
}
