using EventBus.Events;

using System.Text.Json.Serialization;

namespace ServiceB.API.IntegrationEvents.Events
{
    public class MessageSentEvent : IntegrationEvent
    {
        [JsonInclude]
        public string Message { get; set; }
    }
}