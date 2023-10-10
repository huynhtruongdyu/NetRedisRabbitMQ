using EventBus.Bus;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

using ServiceA.API.IntegrationEvents.Events;

using System.Text;

namespace ServiceA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        private readonly IEventBus _eventBus;

        public OrdersController(IDistributedCache cache, IEventBus eventBus)
        {
            _cache = cache;
            _eventBus = eventBus;
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrderAsync()
        {
            var materialCache = await _cache.GetAsync("material");
            if (materialCache == null)
            {
                await _cache.SetAsync("material", BitConverter.GetBytes(10));
            }

            var material = int.Parse(Encoding.UTF8.GetString(await _cache.GetAsync("material")));
            if (material < 0)
            {
                return BadRequest("Not enough material");
            }
            else
            {
                _eventBus.Publish(new CreateOrderEvent());
                await _cache.SetAsync("material", BitConverter.GetBytes(material - 1));
                return Ok("order created");
            }
        }
    }
}