using System;

namespace DataProcessorService.Models;

public class RabbitMqSettings
{
    public string HostName { get; set; }
    public string QueueName { get; set; } 
    public string UserName { get; set; }
    public string Password { get; set; }
}
