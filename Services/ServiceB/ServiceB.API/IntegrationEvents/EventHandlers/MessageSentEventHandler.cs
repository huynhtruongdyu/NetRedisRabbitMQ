using EventBus.Events;

using ServiceB.API.IntegrationEvents.Events;

namespace ServiceB.API.IntegrationEvents.EventHandlers
{
    public class MessageSentEventHandler : IIntegrationEventHandler<MessageSentEvent>
    {
        public Task HandleAsync(MessageSentEvent @event)
        {
            var message = @event.Message;
            Console.WriteLine("message: " + @event.ToString());
            return Task.CompletedTask;
        }
    }
}