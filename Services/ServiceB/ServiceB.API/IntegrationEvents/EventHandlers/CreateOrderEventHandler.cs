using EventBus.Events;

using Microsoft.Extensions.Caching.Distributed;

using ServiceB.API.IntegrationEvents.Events;

using System.Text;

namespace ServiceB.API.IntegrationEvents.EventHandlers
{
    public class CreateOrderEventHandler : IIntegrationEventHandler<CreateOrderEvent>
    {
        private readonly IDistributedCache _cache;

        public CreateOrderEventHandler(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task HandleAsync(CreateOrderEvent @event)
        {
            var cacheValue = await _cache.GetStringAsync("material");
            if (cacheValue != null)
            {
                var material = int.Parse(cacheValue);
                await Console.Out.WriteLineAsync($"Material before: {material}");
                await _cache.SetAsync("material", BitConverter.GetBytes(material - 1));
                Console.WriteLine("Service B: Create Order success");
                await Console.Out.WriteLineAsync($"Material: {material - 1}");
            }
        }
    }
}