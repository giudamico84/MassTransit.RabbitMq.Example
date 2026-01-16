namespace MassTransit.RabbitMq.Common
{
    public class SampleDelayEvent
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
    }
}
