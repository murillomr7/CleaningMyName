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
        try
        {
            byte[] cachedData;
            
            if (cancellationToken == default)
            {
                cachedData = _distributedCache.Get(key);
            }
            else
            {
                cachedData = await _distributedCache.GetAsync(key, cancellationToken);
            }
            
            if (cachedData == null || cachedData.Length == 0)
                return null;

            var jsonString = System.Text.Encoding.UTF8.GetString(cachedData);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultOptions.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = _defaultOptions.SlidingExpiration
        };

        var serializedData = JsonSerializer.Serialize(value);
        var byteData = System.Text.Encoding.UTF8.GetBytes(serializedData);
        
        try
        {
            if (cancellationToken == default)
            {
                _distributedCache.Set(key, byteData, options);
            }
            else
            {
                await _distributedCache.SetAsync(key, byteData, options, cancellationToken);
            }
        }
        catch
        {
            // Log error but don't throw - don't want caching issues to break app
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken == default)
            {
                _distributedCache.Remove(key);
            }
            else
            {
                await _distributedCache.RemoveAsync(key, cancellationToken);
            }
        }
        catch
        {
            // Log error but don't throw - don't want caching issues to break app
        }
    }
}
