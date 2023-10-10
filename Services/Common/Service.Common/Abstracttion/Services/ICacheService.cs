namespace Service.Common.Abstracttion.Services
{
    public interface ICacheService
    {
        T? Get<T>(string cacheKey);

        void Set<T>(string cacheKey, T value);

        bool SetIfNotExists<T>(string cacheKey, T value);

        void Remove(string cachKey);

        void RemoveByPrefix(string prefixKey);
    }
}