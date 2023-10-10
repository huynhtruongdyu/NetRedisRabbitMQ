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
                await _cache.SetStringAsync("material", "10");
            }

            var newMaterialCache = await _cache.GetStringAsync("material");
            if (newMaterialCache == null)
            {
                return BadRequest();
            }

            var material = int.Parse(newMaterialCache);
            if (material == 0)
            {
                return BadRequest("Not enough material");
            }
            else
            {
                //_eventBus.Publish<CreateOrderEvent>(new CreateOrderEvent());
                Console.WriteLine((material - 1).ToString());
                await _cache.SetStringAsync("material", (material - 1).ToString());
                return Ok("order created");
            }
        }
    }
}