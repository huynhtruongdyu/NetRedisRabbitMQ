using Microsoft.AspNetCore.Mvc;

using Service.Common.Abstracttion.Services;

namespace ServiceB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        private readonly ICacheService _cacheService;

        public RedisController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet("{cacheKey}")]
        public async Task<IActionResult> GetAsync([FromRoute] string cacheKey)
        {
            var cacheValue = _cacheService.Get<string>(cacheKey);
            return Ok(cacheValue);
        }

        [HttpPost("{cacheKey}")]
        public async Task<IActionResult> PostAsync([FromRoute] string cacheKey, [FromBody] string value)
        {
            _cacheService.Set<string>(cacheKey, value);
            return Ok();
        }

        [HttpDelete("{cacheKey}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string cacheKey)
        {
            _cacheService.Remove(cacheKey);
            return Ok();
        }

        [HttpDelete("prefix/{prefixKey}")]
        public async Task<IActionResult> DeleteByPrefixAsync([FromRoute] string prefixKey)
        {
            _cacheService.RemoveByPrefix(prefixKey);
            return Ok();
        }
    }
}