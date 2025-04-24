using CleaningMyName.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Xunit;

namespace CleaningMyName.IntegrationTests.Caching;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class RedisCacheIntegrationTests
{
    private readonly ICacheService _cacheService;
    private readonly bool _useRealRedis = false; // true to test w/ Redis

    public RedisCacheIntegrationTests()
    {
        if (_useRealRedis)
        {
            var options = Options.Create(new RedisCacheOptions
            {
                Configuration = "localhost:6379",
                InstanceName = "IntegrationTest:"
            });
            var redisCache = new Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache(options);
            _cacheService = new RedisCacheService(redisCache);
        }
        else
        {
            // Use in-memory cache for testing
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var memDistCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            _cacheService = new RedisCacheService(memDistCache);
        }
    }

    [Fact]
    public async Task SetAndGetAsync_ShouldWorkCorrectly()
    {
        // Arrange
        var key = $"test_key_{Guid.NewGuid()}"; // Unique key for test isolation
        var testData = new TestData
        {
            Id = 123,
            Name = "Integration Test",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _cacheService.SetAsync(key, testData, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testData.Id, result.Id);
        Assert.Equal(testData.Name, result.Name);
        Assert.Equal(testData.CreatedAt.ToUniversalTime().ToString("o"), 
                    result.CreatedAt.ToUniversalTime().ToString("o"));

        // Cleanup
        await _cacheService.RemoveAsync(key);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveItemFromCache()
    {
        // Arrange
        var key = $"test_remove_key_{Guid.NewGuid()}";
        var testData = new TestData { Id = 456, Name = "To Be Removed" };
        await _cacheService.SetAsync(key, testData);

        // Verify item exists
        var initialResult = await _cacheService.GetAsync<TestData>(key);
        Assert.NotNull(initialResult);

        // Act
        await _cacheService.RemoveAsync(key);
        var afterRemoveResult = await _cacheService.GetAsync<TestData>(key);

        // Assert
        Assert.Null(afterRemoveResult);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
