using EventBus.Events;

using ServiceB.API.IntegrationEvents.Events;

namespace ServiceB.API.IntegrationEvents.EventHandlers
{
    public class MessageSentEventHandler : IIntegrationEventHandler<MessageSentEvent>
    {
        public async Task HandleAsync(MessageSentEvent @event)
        {
            await Task.Delay(1000);
            var message = @event.Message;
            Console.WriteLine("message: " + @event.ToString());
        }
    }
}