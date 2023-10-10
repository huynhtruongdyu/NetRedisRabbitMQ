using StackExchange.Redis;

using System.Threading.Tasks;

namespace Service.Common.Abstracttion.Services
{
    public interface ICacheService
    {
        T? Get<T>(string cacheKey);

        void Set<T>(string cacheKey, T value);

        T? SetAndGet<T>(string cacheKey, T value);

        bool SetIfNotExists<T>(string cacheKey, T value);

        void Remove(string cachKey);

        void RemoveByPrefix(string prefixKey);

        bool KeyExists(string key);

        long IncreaseBy(string key, long quantity);

        long DecreaseBy(string key, long quantity);

        //==//
        Task<T?> GetAsync<T>(string cacheKey);

        Task SetAsync<T>(string cacheKey, T value);

        Task<T?> SetAndGetAsync<T>(string cacheKey, T value);

        Task<bool> SetIfNotExistsAsync<T>(string cacheKey, T value);

        Task RemoveAsync(string cachKey);

        Task RemoveByPrefixAsync(string prefixKey);

        Task<bool> KeyExistsAsync(string key);

        Task<long> IncreaseByAsync(string key, long quantity);

        Task<long> DecreaseByAsync(string key, long quantity);

        //==/

        ITransaction CreateTransaction();
    }
}