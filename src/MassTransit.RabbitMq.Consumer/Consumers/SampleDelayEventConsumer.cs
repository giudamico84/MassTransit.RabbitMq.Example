using MassTransit.RabbitMq.Common;
using Microsoft.Extensions.Logging;

namespace MassTransit.RabbitMq.Consumer.Consumers
{
    public class SampleDelayEventConsumer : IConsumer<SampleDelayEvent>
    {
        private readonly ILogger<SampleDelayEventConsumer> _logger;

        public SampleDelayEventConsumer(ILogger<SampleDelayEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<SampleDelayEvent> context)
        {
            _logger.LogInformation("SampleEvent received: {Id} - {Description} - Attempt {Attempt} + {Redelivery}", context.Message.Id, context.Message.Description, context.GetRetryAttempt(), context.GetRedeliveryCount());

            throw new Exception("Simulated exception to demonstrate retry mechanism.");
        }
    }
}
