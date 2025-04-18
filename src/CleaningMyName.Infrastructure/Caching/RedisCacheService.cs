using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CleaningMyName.Infrastructure.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly DistributedCacheEntryOptions _defaultOptions;

    public RedisCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
        _defaultOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var cachedData = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(cachedData))
            return null;

        return JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultOptions.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = _defaultOptions.SlidingExpiration
        };

        var serializedData = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serializedData, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }
}
