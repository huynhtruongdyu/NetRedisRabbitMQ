using EventBus.Bus;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

using Service.Common.Abstracttion.Services;

using ServiceA.API.IntegrationEvents.Events;

using System.Text;

namespace ServiceA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ICacheService _cache;
        private readonly IEventBus _eventBus;

        public OrdersController(ICacheService cache, IEventBus eventBus)
        {
            _cache = cache;
            _eventBus = eventBus;
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrderAsync()
        {
            if (!await _cache.KeyExistsAsync("iphone13"))
            {
                await _cache.SetIfNotExistsAsync<long>("iphone13", 0);
            }

            var slBanra = await _cache.IncreaseByAsync("iphone13", 1);

            if (slBanra > 10)
            {
                Console.WriteLine("het hang");
                await _cache.DecreaseByAsync("iphone13", 1);
                return BadRequest("het hang");
            }

            Console.WriteLine("So luong sau khi ban: " + slBanra);

            //_eventBus.Publish<CreateOrderEvent>(new CreateOrderEvent());
            return Ok("order created");
        }

        [HttpPost("place-order-v2")]
        public async Task<IActionResult> PlaceOrderV2Async()
        {
            await _cache.SetIfNotExistsAsync<long>("STOCK", 10);

            var slTonKho = await _cache.GetAsync<long>("STOCK");
            var slBanRa = 2;
            slTonKho = await _cache.DecreaseByAsync("STOCK", quantity: slBanRa);

            if (slTonKho < 0)
            {
                Console.WriteLine("het hang");
                return BadRequest("het hang");
            }

            Console.WriteLine("So luong sau khi ban: " + slTonKho);

            //_eventBus.Publish<CreateOrderEvent>(new CreateOrderEvent());
            return Ok("order created");
        }

        [HttpPost("place-order-queue")]
        public async Task<IActionResult> PlaceOrderQueueAsync()
        {
            await _cache.SetIfNotExistsAsync("STOCK", 10);
            //Cach 1:
            //var slTonKho = await _cache.GetAsync<long>("STOCK");
            //var slBanRa = 1;
            //if (slTonKho - slBanRa < 0)
            //{
            //    Console.WriteLine("het hang");
            //    return BadRequest("het hang");
            //}

            //Cach 2:
            var slTonKho = await _cache.DecreaseByAsync("STOCK", 1);
            if (slTonKho <= 0)
            {
                await _cache.SetAsync<long>("STOCK", 0);
                Console.WriteLine("het hang");
                return BadRequest("het hang");
            }

            Console.WriteLine("So luong sau khi ban: " + slTonKho);
            var eventPub = new CreateOrderEvent() { Quantity = 1 };
            await Console.Out.WriteLineAsync("=> " + eventPub.ProductId);
            _eventBus.Publish<CreateOrderEvent>(eventPub);
            return Ok("order created");
        }
    }
}