using EventBus.Bus;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ServiceA.API.IntegrationEvents.Events;

namespace ServiceA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RabbitMQController : ControllerBase
    {
        private readonly IEventBus _eventBus;

        public RabbitMQController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        [HttpPost]
        public IActionResult PublishMessageEvent()
        {
            var messageEvent = new MessageSentEvent() { Message = "This is message from ServiceA" };
            _eventBus.Publish<MessageSentEvent>(messageEvent);
            return Ok();
        }
    }
}