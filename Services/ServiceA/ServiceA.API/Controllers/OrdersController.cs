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
                return BadRequest("het hang");
            }

            Console.WriteLine("So luong sau khi ban: " + slBanra);

            //_eventBus.Publish<CreateOrderEvent>(new CreateOrderEvent());
            return Ok("order created");
        }
    }
}