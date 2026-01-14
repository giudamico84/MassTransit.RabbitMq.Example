// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using MassTransit;
using MassTransit.RabbitMq.Common;

// Configure NLog programmatically to log to console and file
var nlogConfig = new LoggingConfiguration();

var consoleTarget = new ConsoleTarget("console")
{
    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${when:when='${scopeproperty:Arguments}'!='':inner=Arguments=${scopeproperty:Arguments}} ${exception:format=ToString}"
};

var fileTarget = new FileTarget("file")
{
    FileName = "${basedir}/logs/app.log",
    ArchiveFileName = "${basedir}/logs/archives/app.{#}.log",
    MaxArchiveFiles = 7,
    ArchiveAboveSize = 10485760, // 10 MB
    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${when:when='${scopeproperty:Arguments}'!='':inner=Arguments=${scopeproperty:Arguments}} ${exception:format=ToString}"
};

nlogConfig.AddTarget(consoleTarget);
nlogConfig.AddTarget(fileTarget);
nlogConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);
nlogConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, fileTarget);

LogManager.Configuration = nlogConfig;

// Setup configuration with fake RabbitMQ settings
var inMemorySettings = new Dictionary<string, string?>
{
    ["RabbitMQ:ConnectionString"] = "amqp://guest:guest@localhost:5672/VISECA"
};

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemorySettings)
    .Build();

// Setup DI
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
    builder.AddNLog(new NLogProviderOptions
    {
        CaptureMessageTemplates = true,
        CaptureMessageProperties = true,
        IncludeScopes = true
    });
});

services.AddMassTransit(x => {     
    x.UsingRabbitMq((context, cfg) =>
    {        
        cfg.Host(new Uri(configuration.GetSection("RabbitMQ:ConnectionString").Value));

        cfg.Message<SampleEvent>(t =>
        {
            t.SetEntityName("_MassTransit.RabbitMq.Common.SampleEvent");
        });
    });
});

var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("RabbitMqConsole");

try
{
    // Create a sample message and publish
    var publisher = provider.GetRequiredService<IPublishEndpoint>();
    
    for (int i = 0; i < 50; i++)
    {
        await Task.Delay(100);

        var message = new SampleEvent { Id = Guid.NewGuid(), Description = $"Hello RabbitMQ {i} - 1" };
        await publisher.Publish(message, CancellationToken.None);
        logger.LogInformation("Published message {Id} with description '{Description}'", message.Id, message.Description);

        message = new SampleEvent { Id = Guid.NewGuid(), Description = $"Hello RabbitMQ {i} - 2" };
        await publisher.Publish(message, CancellationToken.None);
        logger.LogInformation("Published message {Id} with description '{Description}'", message.Id, message.Description);

        message = new SampleEvent { Id = Guid.NewGuid(), Description = $"Hello RabbitMQ {i} - 3" };
        await publisher.Publish(message, CancellationToken.None);
        logger.LogInformation("Published message {Id} with description '{Description}'", message.Id, message.Description);

        logger.LogInformation("Loop iteration {Iteration}", i);
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Unhandled exception during publish loop.");
    throw;
}
finally
{
    await provider.DisposeAsync();
    LogManager.Shutdown();

    logger.LogInformation("Published SampleMessage.");
    Console.ReadLine();
}
