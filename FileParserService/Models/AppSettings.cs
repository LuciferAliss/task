namespace FileParserService.Models;

public class AppSettings
{
    public string InputDirectory { get; set; } = "Input";
    public string ProcessedDirectory { get; set; } = "Processed";
    public string OutputDirectory { get; set; } = "Output";
    public string SearchPattern { get; set; } = "*.xml";
    public int DelayMilliseconds { get; set; } = 1000;
}