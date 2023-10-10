using EventBus.Events;

using ServiceC.API.IntegrationEvents.Events;

namespace ServiceC.API.IntegrationEvents.EventHandlers
{
    public class MessageSentEventHandler : IIntegrationEventHandler<MessageSentEvent>
    {
        public async Task HandleAsync(MessageSentEvent @event)
        {
            Console.WriteLine(DateTime.Now);
            await Task.Delay(3000);
        }
    }
}