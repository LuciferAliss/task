namespace FileParserService.Services;

public interface IRabbitMqPublisher
{
    Task InitializeAsync();
    Task SendMessage(string jsonMessage);
}
