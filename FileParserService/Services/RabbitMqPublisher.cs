using FileParserService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace FileParserService.Services;

public class RabbitMqPublisher(
    IOptions<RabbitMqSettings> settings,
    ILogger<RabbitMqPublisher> logger
) : IRabbitMqPublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger = logger;
    private readonly RabbitMqSettings _settings = settings.Value;
    private IConnection? _connection;

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Подключение к RabbitMQ. Host: {HostName}", _settings.HostName);
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password
        };
        try
        {
            _connection = await factory.CreateConnectionAsync();
            _logger.LogInformation("Соединение с RabbitMQ успешно установлено.");
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogCritical(ex, "Не удалось подключиться к брокеру RabbitMQ. Проверьте адрес, порт и сетевую доступность.");
            throw; 
        }
    }

    public async Task SendMessage(string jsonMessage)
    {
        try
        {   
            _logger.LogInformation("Публикация сообщения в RabbitMQ.");

            if (_connection == null || !_connection.IsOpen)
            {
                throw new InvalidOperationException("Соединение с RabbitMQ не установлено.");
            }

            using var channel = await _connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = System.Text.Encoding.UTF8.GetBytes(jsonMessage);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _settings.QueueName,
                mandatory: true,
                body: body,
                basicProperties: new BasicProperties { Persistent = true });

            _logger.LogInformation("Сообщение опубликовано.");
        }
        catch (InvalidOperationException invOpEx)
        {
            _logger.LogError(invOpEx, "Ошибка при публикации сообщения в RabbitMQ: {Message}", invOpEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось отправить сообщение в RabbitMQ.");
            throw;
        }
    }
}