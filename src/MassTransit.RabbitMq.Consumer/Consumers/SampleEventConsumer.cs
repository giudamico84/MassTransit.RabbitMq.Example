using MassTransit.RabbitMq.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassTransit.RabbitMq.Consumer.Consumers
{
    public class SampleEventConsumer : IConsumer<SampleEvent>
    {
        private readonly ILogger<SampleEventConsumer> _logger;

        public SampleEventConsumer(ILogger<SampleEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<SampleEvent> context)
        {
            _logger.LogInformation("SampleEvent received: {Id} - {Description}", context.Message.Id, context.Message.Description);

            return Task.CompletedTask;
        }
    }
}
