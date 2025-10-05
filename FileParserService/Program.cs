using FileParserService;
using FileParserService.Models;
using FileParserService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<AppSettings>(context.Configuration.GetSection("Settings"));

        services.AddSingleton<IProcessorService, ProcessorService>();
        services.AddSingleton<AppHost>();
    })
    .ConfigureLogging((context, logging) => 
    {
        logging.ClearProviders(); 
        logging.AddConfiguration(context.Configuration.GetSection("Logging"));
        logging.AddConsole();
        logging.AddFile();
    })
    .Build();

var app = host.Services.GetRequiredService<AppHost>();
await app.RunAsync();