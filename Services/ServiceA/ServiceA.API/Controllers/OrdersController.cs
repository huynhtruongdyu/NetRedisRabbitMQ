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
            await _cache.SetIfNotExistsAsync<string>("material", "10");

            var newMaterialCache = await _cache.GetAsync<string>("material");
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
                await _cache.SetAsync<string>("material", (material - 1).ToString());
                Console.WriteLine((material - 1).ToString());
                return Ok("order created");
            }
        }
    }
}