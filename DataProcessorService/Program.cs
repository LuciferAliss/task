using DataProcessorService.Database;
using DataProcessorService.Models;
using DataProcessorService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqSettings>(context.Configuration.GetSection("RabbitMq"));
        services.Configure<DbSettings>(context.Configuration.GetSection("Database"));

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<DbSettings>>().Value;
            options.UseSqlite(dbSettings.ConnectionString);
        });

        services.AddScoped<IDataStorageService, SqliteStorageService>(); 
        services.AddHostedService<RabbitMqListener>();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.AddConfiguration(context.Configuration.GetSection("Logging"));
        logging.AddConsole();
        logging.AddFile();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при применении миграций базы данных.");
        throw;
    }
}

await host.RunAsync();