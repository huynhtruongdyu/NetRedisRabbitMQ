using EventBus.Events;

namespace ServiceA.API.IntegrationEvents.Events
{
    public class CreateOrderEvent : IntegrationEvent
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();
    }
}