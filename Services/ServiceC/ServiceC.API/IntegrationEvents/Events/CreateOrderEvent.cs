using EventBus.Events;

namespace ServiceC.API.IntegrationEvents.Events
{
    public class CreateOrderEvent : IntegrationEvent
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();
    }
}