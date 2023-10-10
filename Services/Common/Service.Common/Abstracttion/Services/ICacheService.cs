namespace Service.Common.Abstracttion.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);

        Task SetAsync<T>(string cacheKey, T value, CancellationToken cancellationToken = default);

        Task<bool> SetIfNotExistsAsync<T>(string cacheKey, T value, CancellationToken cancellationToken = default);

        Task RemoveAsync(string cachKey, CancellationToken cancellationToken = default);

        Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default);
    }
}