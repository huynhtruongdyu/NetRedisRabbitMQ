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
            _cache.SetIfNotExists<int>("material", 10);

            var newMaterialCache = _cache.Get<int>("material");
            if (newMaterialCache == 0)
            {
                return BadRequest();
            }

            //_eventBus.Publish<CreateOrderEvent>(new CreateOrderEvent());
            _cache.Set<int>("material", newMaterialCache - 1);
            Console.WriteLine((newMaterialCache - 1).ToString());
            return Ok("order created");
        }
    }
}