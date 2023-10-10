using Newtonsoft.Json;

using Service.Common.Abstracttion.Services;

using StackExchange.Redis;

namespace Service.Common.Infrastructure.Services.StackExchangeCache
{
    public class StackExchangeCacheService : ICacheService
    {
        private readonly IRedisConnectionFactory _redisConnectionFactory;
        private readonly IDatabase _db;

        public StackExchangeCacheService(IRedisConnectionFactory redisConnectionFactory)
        {
            _redisConnectionFactory = redisConnectionFactory;
            _db = _redisConnectionFactory.Connection().GetDatabase();
        }

        public T? Get<T>(string cacheKey)
        {
            var cacheValue = _db.StringGet(cacheKey);
            if (!cacheValue.HasValue)
            {
                return default(T?);
            }
            //string json = cacheValue.ToString();
            //if (string.IsNullOrEmpty(json))
            //{
            //    return default(T?);
            //}
            //T? value = JsonConvert.DeserializeObject<T>(json);
            //return value;
            return (T)Convert.ChangeType(cacheValue, typeof(T));
        }

        public void Remove(string cachKey)
        {
            _db.KeyDelete(cachKey);
        }

        public void RemoveByPrefix(string prefixKey)
        {
            var keys = getKeysByPrefix(prefixKey);

            foreach (var key in keys)
            {
                _db.KeyDelete(key);
            }
        }

        public void Set<T>(string cacheKey, T value)
        {
            string valueString = JsonConvert.SerializeObject(value);
            _db.StringSet(cacheKey, valueString);
        }

        public bool SetIfNotExists<T>(string cacheKey, T value)
        {
            string valueString = JsonConvert.SerializeObject(value);
            return _db.StringSet(cacheKey, valueString, when: When.NotExists);
        }

        #region private methods

        private IServer getServer()
        {
            var endpoints = _redisConnectionFactory.Connection().GetEndPoints();
            var server = _redisConnectionFactory.Connection().GetServer(endpoints[0]);

            return server;
        }

        private RedisKey[] getKeysByPrefix(string prefix)
        {
            var server = getServer();
            var keys = server.Keys(pattern: prefix + "*");
            return keys.ToArray();
        }

        #endregion private methods
    }
}