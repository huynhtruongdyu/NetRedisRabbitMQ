using EventBus.Events;

using System.Text.Json.Serialization;

namespace ServiceA.API.IntegrationEvents.Events
{
    public class MessageSentEvent : IntegrationEvent
    {
        [JsonInclude]
        public string Message { get; set; }
    }
}