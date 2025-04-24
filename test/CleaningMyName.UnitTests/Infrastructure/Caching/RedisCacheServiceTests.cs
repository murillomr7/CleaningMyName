using CleaningMyName.Infrastructure.Caching;
using CleaningMyName.UnitTests.Common;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;
using FluentAssertions;

namespace CleaningMyName.UnitTests.Infrastructure.Caching;

public class RedisCacheServiceTests : TestBase
{
    private readonly Mock<IDistributedCache> _mockDistributedCache;
    private readonly RedisCacheService _cacheService;

    public RedisCacheServiceTests()
    {
        _mockDistributedCache = new Mock<IDistributedCache>();
        _cacheService = new RedisCacheService(_mockDistributedCache.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnCachedValue()
    {
        // Arrange
        var testKey = "test_key";
        var testObject = new TestData { Id = 1, Name = "Test" };
        var serializedData = JsonSerializer.Serialize(testObject);
        var byteData = Encoding.UTF8.GetBytes(serializedData);

        _mockDistributedCache.Setup(x => x.Get(testKey))
            .Returns(byteData);

        // Act
        var result = await _cacheService.GetAsync<TestData>(testKey);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testObject.Id);
        result.Name.Should().Be(testObject.Name);
        _mockDistributedCache.Verify(x => x.Get(testKey), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var testKey = "nonexistent_key";
        _mockDistributedCache.Setup(x => x.Get(testKey))
            .Returns((byte[])null);

        // Act
        var result = await _cacheService.GetAsync<TestData>(testKey);

        // Assert
        result.Should().BeNull();
        _mockDistributedCache.Verify(x => x.Get(testKey), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldStoreValueInCache()
    {
        // Arrange
        var testKey = "test_key";
        var testObject = new TestData { Id = 1, Name = "Test" };
        var expectedSerializedData = JsonSerializer.Serialize(testObject);

        _mockDistributedCache.Setup(x => x.Set(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>()));

        // Act
        await _cacheService.SetAsync(testKey, testObject);

        // Assert
        _mockDistributedCache.Verify(x => x.Set(
            testKey,
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == expectedSerializedData),
            It.IsAny<DistributedCacheEntryOptions>()), 
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithCustomExpiration_ShouldUseProvidedExpiration()
    {
        // Arrange
        var testKey = "test_key";
        var testObject = new TestData { Id = 1, Name = "Test" };
        var customExpiration = TimeSpan.FromMinutes(10);

        _mockDistributedCache.Setup(x => x.Set(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>()));

        // Act
        await _cacheService.SetAsync(testKey, testObject, customExpiration);

        // Assert
        _mockDistributedCache.Verify(x => x.Set(
            testKey,
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(opt => 
                opt.AbsoluteExpirationRelativeToNow == customExpiration)),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveKeyFromCache()
    {
        // Arrange
        var testKey = "test_key";

        _mockDistributedCache.Setup(x => x.Remove(It.IsAny<string>()));

        // Act
        await _cacheService.RemoveAsync(testKey);

        // Assert
        _mockDistributedCache.Verify(x => x.Remove(testKey), Times.Once);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
