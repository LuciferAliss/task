using FileParserService.Models;
using FileParserService.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileParserService.Services;

internal sealed class ProcessorService(
    ILogger<ProcessorService> logger,
    IOptions<AppSettings> settings,
    IRabbitMqPublisher publisher
) : IProcessorService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private readonly ILogger<ProcessorService> _logger = logger;
    private readonly AppSettings _settings = settings.Value;
    private readonly IRabbitMqPublisher _publisher = publisher;
    private readonly string[] _possibleStates = ["Online", "Run", "NotReady", "Offline"];

    public async Task ProcessFileAsync(string inputFilePath)
    {
        var fileName = Path.GetFileName(inputFilePath);
        _logger.LogInformation("[{FileName}] Начало обработки.", fileName);

        try
        {
            var instrumentStatus = await DeserializeRootXmlAsync(inputFilePath);
            if (instrumentStatus == null) return;

            ParseNestedXml(instrumentStatus);
            UpdateModuleStatesRandomly(instrumentStatus);

            var fileId = Guid.NewGuid().ToString();

            string jsonString = JsonSerializer.Serialize(instrumentStatus, _jsonOptions);
            await _publisher.SendMessage(jsonString);

            MoveFileToProcessed(inputFilePath, fileName, fileId);
        }
        catch (InvalidOperationException xmlEx)
        {
            _logger.LogError(
                xmlEx,
                "[{FileName}] Ошибка парсинга XML. Файл может быть поврежден.",
                fileName);
        }
        catch (IOException ioEx)
        {
            _logger.LogWarning(
                ioEx,
                "[{FileName}] Файл может быть заблокирован. Пропускаем до следующего цикла.",
                fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[{FileName}] Неизвестная ошибка при обработке файла.",
                fileName);
        }
    }

    private async Task<InstrumentStatus?> DeserializeRootXmlAsync(string path)
    {
        await using var fileStream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        var status = XmlProcessor.Deserialize<InstrumentStatus>(fileStream);
        if (status == null)
        {
            _logger.LogWarning(
                "Не удалось десериализовать корневой XML из файла {FilePath}",
                path);
            return null;
        }
        return status;
    }

    private void ParseNestedXml(InstrumentStatus status)
    {
        _logger.LogInformation("Парсинг вложенных XML статусов модулей...");
        foreach (var device in status.DeviceStatuses)
        {
            if (string.IsNullOrEmpty(device.RapidControlStatus)) continue;

            using var stringReader = new StringReader(device.RapidControlStatus);
            device.ParsedStatus = device.ModuleCategoryID switch
            {
                "SAMPLER" => XmlProcessor.Deserialize<CombinedSamplerStatus>(stringReader),
                "QUATPUMP" => XmlProcessor.Deserialize<CombinedPumpStatus>(stringReader),
                "COLCOMP" => XmlProcessor.Deserialize<CombinedOvenStatus>(stringReader),
                _ => null
            };
        }
    }

    private void UpdateModuleStatesRandomly(InstrumentStatus status)
    {
        if (status.DeviceStatuses == null) return;

        _logger.LogInformation("Обновление состояний модулей...");
        foreach (var device in status.DeviceStatuses)
        {
            if (device.ParsedStatus != null)
            {
                int randomIndex = Random.Shared.Next(_possibleStates.Length);
                device.ParsedStatus.ModuleState = _possibleStates[randomIndex];
            }
        }
    }

    private void MoveFileToProcessed(string originalPath, string fileName, string fileId)
    {
        var uniqueDestPath = Path.Combine(
            _settings.ProcessedDirectory,
            $"{Path.GetFileNameWithoutExtension(fileName)}_{fileId}{Path.GetExtension(fileName)}"
        );
        File.Move(originalPath, uniqueDestPath);
        _logger.LogInformation("[{FileName}] Файл перемещен в {ProcessedDir}", fileName, _settings.ProcessedDirectory);
    }
}