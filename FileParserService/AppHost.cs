using FileParserService.Models;
using FileParserService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileParserService;

internal sealed class AppHost(
    IProcessorService processor,
    IOptions<AppSettings> settings,
    ILogger<AppHost> logger)
{
    private readonly IProcessorService _processor = processor;
    private readonly AppSettings _settings = settings.Value;
    private readonly ILogger<AppHost> _logger = logger;

    public async Task RunAsync()
    {
        _logger.LogInformation("Сервис запущен. Мониторинг папки: {InputDir}", _settings.InputDirectory);
        CreateDirectories();
        while (true)
        {
            try
            {
                var files = Directory.GetFiles(_settings.InputDirectory, _settings.SearchPattern);
                if (files.Length > 0)
                {
                    _logger.LogInformation("Обнаружено {FileCount} файлов для обработки.", files.Length);

                    var processingTasks = new List<Task>();
                    foreach (var file in files)
                    {
                        processingTasks.Add(Task.Run(() => _processor.ProcessFileAsync(file)));
                    }

                    await Task.WhenAll(processingTasks);
                    _logger.LogInformation("Обработка файлов завершена.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла критическая ошибка в цикле мониторинга.");
            }

            await Task.Delay(_settings.DelayMilliseconds);
        }
    }

    private void CreateDirectories()
    {
        Directory.CreateDirectory(_settings.InputDirectory);
        Directory.CreateDirectory(_settings.ProcessedDirectory);
    }
}