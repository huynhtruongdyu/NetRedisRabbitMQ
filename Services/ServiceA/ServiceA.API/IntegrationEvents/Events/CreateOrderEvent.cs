using EventBus.Events;

using System.Text.Json.Serialization;

namespace ServiceA.API.IntegrationEvents.Events
{
    public class CreateOrderEvent : IntegrationEvent
    {
        [JsonInclude]
        public Guid ProductId { get; set; } = Guid.NewGuid();

        [JsonInclude]
        public int Quantity { get; set; } = 1;
    }
}