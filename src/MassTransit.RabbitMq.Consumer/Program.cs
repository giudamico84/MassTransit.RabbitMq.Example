using MassTransit;
using MassTransit.RabbitMq.Common;
using MassTransit.RabbitMq.Consumer.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using Microsoft.Extensions.Hosting;

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
    ["RabbitMQ:ConnectionString"] = "amqp://guest:guest@localhost:5672/TEST"
};

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemorySettings)
    .Build();

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
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

        services.AddMassTransit(x =>
        {
            x.AddConsumer<SampleEventConsumer>();
            x.AddConsumer<SampleDelayEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {               

                cfg.Host(new Uri(configuration.GetSection("RabbitMQ:ConnectionString").Value));

                cfg.Message<SampleEvent>(t =>
                {
                    t.SetEntityName("_MassTransit.RabbitMq.Common.SampleEvent");                    
                });

                cfg.ReceiveEndpoint("_MassTransit.RabbitMq.Common.SampleEvent.Queue", e =>
                {                    
                    e.Durable = true;
                    e.AutoDelete = false;
                    e.ConfigureConsumer<SampleEventConsumer>(context);                    
                });

                cfg.Message<SampleDelayEvent>(t =>
                {
                    t.SetEntityName("_MassTransit.RabbitMq.Common.SampleDelayEvent");
                });
                                                
                cfg.ReceiveEndpoint("_MassTransit.RabbitMq.Common.SampleDelayEvent.Queue", e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                    e.Durable = true;
                    e.AutoDelete = false;
                    e.ConfigureConsumer<SampleDelayEventConsumer>(context);
                });
            });
        });

    }).Build();    

await host.RunAsync();
