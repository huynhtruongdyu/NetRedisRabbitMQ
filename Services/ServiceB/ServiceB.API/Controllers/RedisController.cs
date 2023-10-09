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
            var cacheValue = await _cacheService.GetAsync<string>(cacheKey);
            return Ok(cacheValue);
        }

        [HttpPost("{cacheKey}")]
        public async Task<IActionResult> PostAsync([FromRoute] string cacheKey, [FromBody] string value)
        {
            await _cacheService.SetAsync<string>(cacheKey, value);
            return Ok();
        }

        [HttpDelete("{cacheKey}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string cacheKey)
        {
            await _cacheService.RemoveAsync(cacheKey);
            return Ok();
        }

        [HttpDelete("prefix/{prefixKey}")]
        public async Task<IActionResult> DeleteByPrefixAsync([FromRoute] string prefixKey)
        {
            await _cacheService.RemoveByPrefixAsync(prefixKey);
            return Ok();
        }
    }
}