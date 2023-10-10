using Microsoft.Extensions.Caching.Distributed;

using Newtonsoft.Json;

using Service.Common.Abstracttion.Services;

using System.Collections.Concurrent;
using System.Text;
using System.Threading;

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

        public T? Get<T>(string cacheKey)
        {
            byte[]? cacheValue = _distributedCache.Get(cacheKey);
            if (cacheValue == null)
            {
                return default(T?);
            }
            T? value = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(cacheValue));
            return value;
        }

        public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            byte[]? cacheValue = await _distributedCache.GetAsync(cacheKey, cancellationToken);
            if (cacheValue == null)
            {
                return default(T?);
            }
            T? value = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(cacheValue));
            return value;
        }

        public void Remove(string cachKey)
        {
            _distributedCache.Remove(cachKey);
            CacheKeys.TryRemove(cachKey, out bool _);
        }

        public async Task RemoveAsync(string cachKey, CancellationToken cancellationToken = default)
        {
            await _distributedCache.RemoveAsync(cachKey, cancellationToken);
            CacheKeys.TryRemove(cachKey, out bool _);
        }

        public void RemoveByPrefix(string prefixKey)
        {
            foreach (var key in CacheKeys.Keys.Where(key => key.StartsWith(prefixKey)))
            {
                Remove(key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task> removeAsyncTasks = CacheKeys.Keys.Where(key => key.StartsWith(prefixKey)).Select(key => RemoveAsync(key, cancellationToken));
            await Task.WhenAll(removeAsyncTasks);
        }

        public void Set<T>(string cacheKey, T value)
        {
            string cacheValue = JsonConvert.SerializeObject(value);
            _distributedCache.Set(cacheKey, Encoding.UTF8.GetBytes(cacheValue));
            CacheKeys.TryAdd(cacheKey, false);
        }

        public async Task SetAsync<T>(string cacheKey, T value, CancellationToken cancellationToken = default)
        {
            string cacheValue = JsonConvert.SerializeObject(value);
            await _distributedCache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(cacheValue), cancellationToken);
            CacheKeys.TryAdd(cacheKey, false);
        }

        public bool SetIfNotExists<T>(string cacheKey, T value)
        {
            var existsValue = _distributedCache.GetString(cacheKey);
            if (existsValue != null)
            {
                return false;
            }
            string cacheValue = JsonConvert.SerializeObject(value);
            _distributedCache.Set(cacheKey, Encoding.UTF8.GetBytes(cacheValue));
            CacheKeys.TryAdd(cacheKey, false);
            return true;
        }

        public async Task<bool> SetIfNotExistsAsync<T>(string cacheKey, T value, CancellationToken cancellationToken = default)
        {
            var existsValue = await _distributedCache.GetStringAsync(cacheKey);
            if (existsValue != null)
            {
                return false;
            }
            string cacheValue = JsonConvert.SerializeObject(value);
            await _distributedCache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(cacheValue), cancellationToken);
            CacheKeys.TryAdd(cacheKey, false);
            return true;
        }
    }
}