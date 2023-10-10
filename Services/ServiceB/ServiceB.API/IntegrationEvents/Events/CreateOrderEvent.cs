using EventBus.Events;

using System.Text.Json.Serialization;

namespace ServiceB.API.IntegrationEvents.Events
{
    public class CreateOrderEvent : IntegrationEvent
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; } = 1;
    }
}