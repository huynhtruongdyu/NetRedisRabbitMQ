using EventBus.Events;

namespace ServiceB.API.IntegrationEvents.Events
{
    public class CreateOrderEvent : IntegrationEvent
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();
    }
}