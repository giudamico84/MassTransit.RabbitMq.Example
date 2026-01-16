using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace MassTransit.RabbitMq.Publisher.Commands
{
    public class PublishSampleDelayEventCommand : Command
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PublishSampleDelayEventCommand> _logger;

        public PublishSampleDelayEventCommand(IPublishEndpoint publishEndpoint, ILogger<PublishSampleDelayEventCommand> logger) : base("publish-sample-delay-event", "Publishes a SampleDelayEvent to RabbitMQ")
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;

            this.Options.Add(new Option<int>("--number_event", "-num") { Description = "Number event to send", DefaultValueFactory = _ => 1 });

            this.SetAction(SetHandler);
        }

        private async Task SetHandler(ParseResult parseResult)
        {
            for (int i = 0; i < parseResult.GetValue<int>("--number_event"); i++)
            {
                var message = new Common.SampleDelayEvent
                {
                    Id = Guid.NewGuid(),
                    Description = $"Hello RabbitMQ {i}"
                };

                await _publishEndpoint.Publish<MassTransit.RabbitMq.Common.SampleDelayEvent>(message);
                _logger.LogInformation("Published SampleDelayEvent {Id} with description '{Description}'", message.Id, message.Description);
            }
        }
    }
}
