using Microsoft.Extensions.Caching.Distributed;

using Newtonsoft.Json;

using Service.Common.Abstracttion.Services;

using System.Collections.Concurrent;

namespace Service.Common.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private static readonly ConcurrentDictionary<string, bool> CacheKeys = new();
        private readonly IDistributedCache _distributedCache;

        public CacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            string? cacheValue = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (cacheValue == null)
            {
                return default(T?);
            }
            T? value = JsonConvert.DeserializeObject<T>(cacheValue);
            return value;
        }

        public async Task RemoveAsync(string cachKey, CancellationToken cancellationToken = default)
        {
            await _distributedCache.RemoveAsync(cachKey, cancellationToken);
            CacheKeys.TryRemove(cachKey, out bool _);
        }

        public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task> removeAsyncTasks = CacheKeys.Keys.Where(key => key.StartsWith(prefixKey)).Select(key => RemoveAsync(key, cancellationToken));
            await Task.WhenAll(removeAsyncTasks);
        }

        public async Task SetAsync<T>(string cacheKey, T value, CancellationToken cancellationToken = default)
        {
            string cacheValue = JsonConvert.SerializeObject(value);
            await _distributedCache.SetStringAsync(cacheKey, cacheValue, cancellationToken);
            CacheKeys.TryAdd(cacheKey, false);
        }
    }
}