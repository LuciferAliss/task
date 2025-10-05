using DataProcessorService.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel;
using System.Text;
using System.Text.Json;

namespace DataProcessorService.Services;

public class RabbitMqListener(
    IOptions<RabbitMqSettings> settings,
    ILogger<RabbitMqListener> logger,
    IServiceScopeFactory scopeFactory) : IHostedService
{
    private readonly ILogger<RabbitMqListener> _logger = logger;
    private readonly RabbitMqSettings _settings = settings.Value;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageReceived; 

            await _channel.BasicConsumeAsync(
                queue: _settings.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Подключение к RabbitMQ установлено и ожидается получение сообщений.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Ошибка при запуске RabbitMQ Listener.");
        }
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        var body = eventArgs.Body.ToArray();
        var jsonMessage = Encoding.UTF8.GetString(body);
        _logger.LogInformation("Получено новое сообщение.");

        try
        {
            var status = JsonSerializer.Deserialize<InstrumentStatus>(jsonMessage);
            if (status?.DeviceStatuses != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var storageService = scope.ServiceProvider.GetRequiredService<IDataStorageService>();
                
                foreach (var device in status.DeviceStatuses)
                {
                    await storageService.SaveDeviceStatusAsync(device);
                }
            }
            
            if (_channel != null)
            {
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
            }
            _logger.LogInformation("Сообщение успешно обработано и подтверждено.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке сообщения из RabbitMQ.");
            if (_channel != null)
            {
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, false);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_channel != null)
            {
                await _channel.CloseAsync(cancellationToken);
                _channel.Dispose();
            }
            if (_connection != null)
            {
                await _connection.CloseAsync(cancellationToken);
                _connection.Dispose();
            }
            _logger.LogInformation("Соединение с RabbitMQ закрыто.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при остановке RabbitMQ Listener.");
        }
    }
}