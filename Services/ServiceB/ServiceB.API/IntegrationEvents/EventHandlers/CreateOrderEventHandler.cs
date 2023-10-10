using EventBus.Events;

using Microsoft.Extensions.Caching.Distributed;

using Service.Common.Abstracttion.Services;

using ServiceB.API.IntegrationEvents.Events;

using System;
using System.Text;

namespace ServiceB.API.IntegrationEvents.EventHandlers
{
    public class CreateOrderEventHandler : IIntegrationEventHandler<CreateOrderEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        public CreateOrderEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task HandleAsync(CreateOrderEvent @event)
        {
            using var scope = _serviceProvider.CreateScope();
            var _cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

            //Cach 1:
            //var cacheValue = await _cache.GetAsync<long>("STOCK");
            //if (await _cache.DecreaseByAsync("STOCK", @event.Quantity) > 0)
            //{
            //    Console.WriteLine("Create Order Success");
            //}
            //else
            //{
            //    Console.WriteLine("out of stock");
            //    await _cache.IncreaseByAsync("STOCK", @event.Quantity);
            //}

            //Cach 2:
            await Task.Delay(new Random().Next(500, 3000));
            var createSuccess = new Random().Next(2) == 0;
            if (createSuccess)
            {
                Console.WriteLine("Create Order Success: " + @event.ProductId);
            }
            else
            {
                Console.WriteLine("Create Order Failed: " + @event.ProductId);
                await _cache.IncreaseByAsync("STOCK", @event.Quantity);
            }
        }
    }
}