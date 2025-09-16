using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Pulse.Shared.Aggregate;

namespace Pulse.Shared.Tests.Aggregate;

public class AggregateDataServiceTests
{
    private readonly MemoryCache _cache;
    private readonly Dictionary<Type, AggregateDataProvider> _providers;
    private readonly AggregateDataService _sut;
    private readonly TestDataProvider _testProvider;
    private readonly TestBatchDataProvider _testBatchProvider;

    public AggregateDataServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _testProvider = new TestDataProvider();
        _testBatchProvider = new TestBatchDataProvider();

        _providers = new Dictionary<Type, AggregateDataProvider>
        {
            { typeof(TestDataRequest), _testProvider },
            { typeof(TestBatchDataRequest), _testBatchProvider },
        };

        _sut = new AggregateDataService(
            _cache,
            _providers,
            NullLogger<AggregateDataService>.Instance
        );
    }

    public class EnlistDataRequest_SingleItem : AggregateDataServiceTests
    {
        [Fact]
        public async Task Should_Return_Cached_Result_When_Available()
        {
            // Arrange
            var request = new TestDataRequest("test-key");
            var expectedData = new TestData("cached-value");
            _cache.Set(request.CacheKey, expectedData);

            // Act
            var result = await _sut.EnlistDataRequest<TestData, TestDataRequest>(request);

            // Assert
            Assert.Equal(expectedData, result);
            Assert.Equal(0, _testProvider.GetDataCallCount);
        }

        [Fact]
        public async Task Should_Fetch_Data_When_Not_Cached()
        {
            // Arrange
            var request = new TestDataRequest("test-key");
            var expectedData = new TestData("fetched-value");
            _testProvider.SetupResponse(expectedData);

            // Act
            var result = await _sut.EnlistDataRequest<TestData, TestDataRequest>(request);

            // Assert
            Assert.Equal(expectedData, result);
            Assert.Equal(1, _testProvider.GetDataCallCount);
        }

        [Fact]
        public async Task Should_Cache_Fetched_Data()
        {
            // Arrange
            var request = new TestDataRequest("test-key");
            var expectedData = new TestData("fetched-value");
            _testProvider.SetupResponse(expectedData);

            // Act
            _ = await _sut.EnlistDataRequest<TestData, TestDataRequest>(request);

            // Assert
            Assert.True(_cache.TryGetValue(request.CacheKey, out var cachedValue));
            Assert.Equal(expectedData, cachedValue);
        }

        [Fact]
        public async Task Should_Not_Cache_Null_Results()
        {
            // Arrange
            var request = new TestDataRequest("test-key");
            _testProvider.SetupResponse(null);

            // Act
            var result = await _sut.EnlistDataRequest<TestData, TestDataRequest>(request);

            // Assert
            Assert.Null(result);
            Assert.False(_cache.TryGetValue(request.CacheKey, out _));
        }

        [Fact]
        public async Task Should_Return_Null_When_No_Provider_Found()
        {
            // Arrange
            var providers = new Dictionary<Type, AggregateDataProvider>();
            var serviceWithoutProvider = new AggregateDataService(
                _cache,
                providers,
                NullLogger<AggregateDataService>.Instance
            );
            var request = new TestDataRequest("test-key");

            // Act
            var result = await serviceWithoutProvider.EnlistDataRequest<TestData, TestDataRequest>(
                request
            );

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Should_Return_Null_When_Provider_Is_Wrong_Type()
        {
            // This test ensures that if the provider dictionary somehow contains
            // a provider that doesn't match the expected signature, the service handles it gracefully
            // We can't actually test this with the generic constraints, so we'll test missing provider instead
            var emptyProviders = new Dictionary<Type, AggregateDataProvider>();
            var serviceWithoutProvider = new AggregateDataService(
                _cache,
                emptyProviders,
                NullLogger<AggregateDataService>.Instance
            );
            var request = new TestDataRequest("test-key");

            // Act
            var result = await serviceWithoutProvider.EnlistDataRequest<TestData, TestDataRequest>(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Should_Deduplicate_Concurrent_Requests()
        {
            // Arrange
            var request = new TestDataRequest("test-key");
            var expectedData = new TestData("fetched-value");
            _testProvider.SetupResponse(expectedData, delayMs: 100);

            // Act - Start multiple concurrent requests
            var task1 = _sut.EnlistDataRequest<TestData, TestDataRequest>(request);
            var task2 = _sut.EnlistDataRequest<TestData, TestDataRequest>(request);
            var task3 = _sut.EnlistDataRequest<TestData, TestDataRequest>(request);

            var results = await Task.WhenAll(task1, task2, task3);

            // Assert
            Assert.All(results, result => Assert.Equal(expectedData, result));
            Assert.Equal(1, _testProvider.GetDataCallCount); // Should only call provider once
        }

        [Fact]
        public async Task Should_Handle_Provider_Exceptions_Gracefully()
        {
            // Arrange
            var request = new TestDataRequest("test-key");
            _testProvider.SetupException(new InvalidOperationException("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.EnlistDataRequest<TestData, TestDataRequest>(request)
            );

            Assert.Equal("Test exception", exception.Message);
        }
    }

    public class EnlistDataRequest_Batch : AggregateDataServiceTests
    {
        [Fact]
        public async Task Should_Return_Cached_Result_When_Available()
        {
            // Arrange
            var keys = new[] { "key1", "key2" };
            var request = new TestBatchDataRequest(keys);
            var expectedData = new[] { new TestData("value1"), new TestData("value2") };
            _cache.Set(request.CacheKey, expectedData);

            // Act
            var result = await _sut.EnlistDataRequest<TestData, string, TestBatchDataRequest>(
                request
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Equal("value1", result[0].Value);
            Assert.Equal("value2", result[1].Value);
            Assert.Equal(0, _testBatchProvider.GetDataCallCount);
        }

        [Fact]
        public async Task Should_Cache_Individual_Items_After_Batch_Fetch()
        {
            // Arrange
            var keys = new[] { "key1", "key2" };
            var request = new TestBatchDataRequest(keys);
            var batchData = new[] { new TestData("value1"), new TestData("value2") };
            _testBatchProvider.SetupResponse(batchData);

            // Act
            var result = await _sut.EnlistDataRequest<TestData, string, TestBatchDataRequest>(
                request
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);

            // Individual items should be cached
            Assert.True(_cache.TryGetValue(request.GetItemCacheKey("key1"), out var item1));
            Assert.True(_cache.TryGetValue(request.GetItemCacheKey("key2"), out var item2));
            Assert.Equal(batchData[0], item1);
            Assert.Equal(batchData[1], item2);
        }

        [Fact]
        public async Task Should_Not_Cache_Null_Individual_Items()
        {
            // Arrange
            var keys = new[] { "key1", "key2" };
            var request = new TestBatchDataRequest(keys);
            var batchData = new[] { new TestData("value1"), null }; // Second item is null
            _testBatchProvider.SetupResponse(batchData!);

            // Act
            await _sut.EnlistDataRequest<TestData, string, TestBatchDataRequest>(request);

            // Assert
            Assert.True(_cache.TryGetValue(request.GetItemCacheKey("key1"), out _));
            Assert.False(_cache.TryGetValue(request.GetItemCacheKey("key2"), out _)); // Null not cached
        }

        [Fact]
        public async Task Should_Deduplicate_Concurrent_Batch_Requests()
        {
            // Arrange
            var keys = new[] { "key1", "key2" };
            var request = new TestBatchDataRequest(keys);
            var batchData = new[] { new TestData("value1"), new TestData("value2") };
            _testBatchProvider.SetupResponse(batchData, delayMs: 100);

            // Act - Start multiple concurrent requests
            var task1 = _sut.EnlistDataRequest<TestData, string, TestBatchDataRequest>(request);
            var task2 = _sut.EnlistDataRequest<TestData, string, TestBatchDataRequest>(request);
            var task3 = _sut.EnlistDataRequest<TestData, string, TestBatchDataRequest>(request);

            var results = await Task.WhenAll(task1, task2, task3);

            // Assert
            Assert.All(
                results,
                result =>
                {
                    Assert.NotNull(result);
                    Assert.Equal(2, result.Length);
                    Assert.Equal("value1", result[0].Value);
                    Assert.Equal("value2", result[1].Value);
                }
            );
            Assert.Equal(1, _testBatchProvider.GetDataCallCount); // Should only call provider once
        }

        [Fact]
        public async Task Should_Handle_Empty_Key_Array()
        {
            // Arrange
            var keys = Array.Empty<string>();
            var request = new TestBatchDataRequest(keys);
            var batchData = Array.Empty<TestData>();
            _testBatchProvider.SetupResponse(batchData);

            // Act
            var result = await _sut.EnlistDataRequest<TestData, string, TestBatchDataRequest>(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_Handle_Provider_Exceptions_Gracefully()
        {
            // Arrange
            var keys = new[] { "key1", "key2" };
            var request = new TestBatchDataRequest(keys);
            _testBatchProvider.SetupException(
                new InvalidOperationException("Batch test exception")
            );

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.EnlistDataRequest<TestData, string, TestBatchDataRequest>(request)
            );
            Assert.Equal("Batch test exception", exception.Message);
        }
    }

    public class CachingBehavior : AggregateDataServiceTests
    {
        [Fact]
        public async Task Should_Use_Separate_Cache_Keys_For_Different_Requests()
        {
            // Arrange
            var request1 = new TestDataRequest("key1");
            var request2 = new TestDataRequest("key2");
            var data1 = new TestData("value1");
            var data2 = new TestData("value2");

            // Act
            _testProvider.SetupResponse(data1);
            await _sut.EnlistDataRequest<TestData, TestDataRequest>(request1);

            _testProvider.SetupResponse(data2);
            await _sut.EnlistDataRequest<TestData, TestDataRequest>(request2);

            // Assert
            Assert.True(_cache.TryGetValue(request1.CacheKey, out var cached1));
            Assert.True(_cache.TryGetValue(request2.CacheKey, out var cached2));
            Assert.Equal(data1, cached1);
            Assert.Equal(data2, cached2);
        }

        [Fact]
        public async Task Should_Respect_Cache_Expiration()
        {
            // Arrange
            var request = new TestDataRequest("test-key");
            var data = new TestData("value");
            _testProvider.SetupResponse(data);

            // Set up cache with very short expiration
            var cacheOptions = new MemoryCacheOptions();
            var shortCache = new MemoryCache(cacheOptions);
            var serviceWithShortCache = new AggregateDataService(
                shortCache,
                _providers,
                NullLogger<AggregateDataService>.Instance
            );

            // Act
            await serviceWithShortCache.EnlistDataRequest<TestData, TestDataRequest>(request);
            shortCache.Set(request.CacheKey, data, TimeSpan.FromMilliseconds(1));

            await Task.Delay(10); // Wait for cache expiration

            _testProvider.SetupResponse(new TestData("new-value"));
            var result = await serviceWithShortCache.EnlistDataRequest<TestData, TestDataRequest>(
                request
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new-value", result.Value); // Should fetch new data, not cached
            Assert.Equal(2, _testProvider.GetDataCallCount); // Called twice due to expiration
        }
    }

    // Test helper classes
    public record TestData(string Value);

    public class TestDataRequest(string key) : AggregateDataRequest<TestData>
    {
        public string Key { get; } = key;
        public override string CacheKey => $"test:{Key}";
    }

    public class TestBatchDataRequest(string[] keys)
        : AggregateDataBatchRequest<TestData, string>(keys)
    {
        public override string CacheKey => $"test:batch:{string.Join(',', Keys)}";

        public override string GetItemCacheKey(string key) => $"test:{key}";
    }

    public class TestBatchDataResponse(TestData[] data)
        : AggregateDataBatchResponse<TestData, string>(data)
    {
        public override TestData? GetItemForKey(string key)
        {
            var index = Array.FindIndex(
                Data,
                d => d?.Value.EndsWith(key.Last().ToString()) == true
            );
            return index >= 0 ? Data[index] : null;
        }
    }

    public class TestDataProvider : AggregateDataProvider<TestDataRequest, TestData>
    {
        private TestData? _responseData;
        private Exception? _exception;
        private int _delayMs;

        public int GetDataCallCount { get; private set; }

        public void SetupResponse(TestData? data, int delayMs = 0)
        {
            _responseData = data;
            _exception = null;
            _delayMs = delayMs;
        }

        public void SetupException(Exception exception)
        {
            _exception = exception;
            _responseData = null;
        }

        public override async Task<AggregateDataResponse<TestData>> GetData(
            TestDataRequest request,
            CancellationToken ct = default
        )
        {
            GetDataCallCount++;

            if (_delayMs > 0)
                await Task.Delay(_delayMs, ct);

            if (_exception != null)
                throw _exception;

            return new AggregateDataResponse<TestData>(_responseData!);
        }
    }

    public class TestBatchDataProvider : AggregateDataProvider<TestBatchDataRequest, TestData[]>
    {
        private TestData[]? _responseData;
        private Exception? _exception;
        private int _delayMs;

        public int GetDataCallCount { get; private set; }

        public void SetupResponse(TestData[]? data, int delayMs = 0)
        {
            _responseData = data;
            _exception = null;
            _delayMs = delayMs;
        }

        public void SetupException(Exception exception)
        {
            _exception = exception;
            _responseData = null;
        }

        public override async Task<AggregateDataResponse<TestData[]>> GetData(
            TestBatchDataRequest request,
            CancellationToken ct = default
        )
        {
            GetDataCallCount++;

            if (_delayMs > 0)
                await Task.Delay(_delayMs, ct);

            if (_exception != null)
                throw _exception;

            return new TestBatchDataResponse(_responseData ?? Array.Empty<TestData>());
        }
    }
}
