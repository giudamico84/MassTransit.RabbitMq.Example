using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace MassTransit.RabbitMq.Publisher.Commands
{
    public class PublishSampleEventCommand : Command
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PublishSampleEventCommand> _logger;

        public PublishSampleEventCommand(IPublishEndpoint publishEndpoint, ILogger<PublishSampleEventCommand> logger) : base("publish-sample-event", "Publishes a SampleEvent to RabbitMQ")
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
                var message = new Common.SampleEvent
                {
                    Id = Guid.NewGuid(),
                    Description = $"Hello RabbitMQ {i}"
                };

                await _publishEndpoint.Publish<MassTransit.RabbitMq.Common.SampleEvent>(message);
                _logger.LogInformation("Published SampleEvent {Id} with description '{Description}'", message.Id, message.Description);
            }
        }        
    }
}
