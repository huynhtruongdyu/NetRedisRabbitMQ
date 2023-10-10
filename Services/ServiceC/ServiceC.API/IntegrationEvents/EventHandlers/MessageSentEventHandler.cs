using EventBus.Events;

using ServiceC.API.IntegrationEvents.Events;

namespace ServiceC.API.IntegrationEvents.EventHandlers
{
    public class MessageSentEventHandler : IIntegrationEventHandler<MessageSentEvent>
    {
        public async Task HandleAsync(MessageSentEvent @event)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString());
            await Task.Delay(1000);
            var message = @event.Message;
            Console.WriteLine("message: " + @event.ToString());
        }
    }
}