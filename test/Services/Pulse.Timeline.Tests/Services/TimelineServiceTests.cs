using FluentAssertions;
using NSubstitute;
using Pulse.Timeline.Services;
using StackExchange.Redis;

namespace Pulse.Timeline.Tests;

public class TimelineServiceTests
{
    private readonly IDatabase _redis;
    private readonly TimelineService _timelineService;

    public TimelineServiceTests()
    {
        _redis = Substitute.For<IDatabase>();
        _timelineService = new TimelineService(_redis);
    }

    [Fact]
    public async Task GetTimelinePage_WhenUserIdIsEmpty_ReturnsEmptyList()
    {
        // Act
        var result = await _timelineService.GetTimelinePage(Guid.Empty, "cursor", 5);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTimelinePage_WhenCountIsZero_ReturnsEmptyList()
    {
        // Act
        var result = await _timelineService.GetTimelinePage(Guid.NewGuid(), "cursor", 0);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTimelinePage_WhenCursorIsNull_ReturnsCorrectPostIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var count = 5;

        var expectedPostIds = Enumerable.Range(1, count).Select(i => Guid.NewGuid()).ToList();

        var redisValues = expectedPostIds.Select(id => (RedisValue)$"post:{id}").ToArray();

        _redis
            .SortedSetRangeByRankAsync($"timeline:{userId}", 0, count, Order.Descending)
            .Returns(redisValues);

        // Act
        var postIds = await _timelineService.GetTimelinePage(userId, null, count);

        // Assert
        postIds.Should().BeEquivalentTo(expectedPostIds);
    }

    [Fact]
    public async Task GetTimelinePage_WhenCursorIsNotNull_ReturnsCorrectPostIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cursor = "cursor";
        var count = 5;

        var expectedPostIds = Enumerable.Range(1, count).Select(i => Guid.NewGuid()).ToList();

        var redisValues = expectedPostIds.Select(id => (RedisValue)$"post:{id}").ToArray();

        _redis.SortedSetRank(userId.ToString(), cursor).Returns(0);
        _redis
            .SortedSetRangeByRankAsync($"timeline:{userId}", 1, count + 1, Order.Descending)
            .Returns(redisValues);

        // Act
        var postIds = await _timelineService.GetTimelinePage(userId, cursor, count);

        // Assert
        postIds.Should().BeEquivalentTo(expectedPostIds);
    }

    [Fact]
    public async Task GetTimelinePage_WhenSortedSetRankReturnsNonZeroRank_ReturnsCorrectPostIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cursor = "cursor";
        var count = 5;

        var expectedPostIds = Enumerable.Range(1, count).Select(i => Guid.NewGuid()).ToList();

        var redisValues = expectedPostIds.Select(id => (RedisValue)$"post:{id}").ToArray();

        _redis.SortedSetRank(userId.ToString(), cursor).Returns(1);
        _redis
            .SortedSetRangeByRankAsync($"timeline:{userId}", 2, count + 2, Order.Descending)
            .Returns(redisValues);

        // Act
        var postIds = await _timelineService.GetTimelinePage(userId, cursor, count);

        // Assert
        postIds.Should().BeEquivalentTo(expectedPostIds);
    }

    [Fact]
    public async Task GetTimelinePage_WhenSortedSetRangeByRankAsyncReturnsEmptyArray_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cursor = "cursor";
        var count = 5;

        _redis.SortedSetRank(userId.ToString(), cursor).Returns(0);
        _redis
            .SortedSetRangeByRankAsync($"timeline:{userId}", 1, count + 1, Order.Descending)
            .Returns(new RedisValue[0]);

        // Act
        var postIds = await _timelineService.GetTimelinePage(userId, cursor, count);

        // Assert
        postIds.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTimelinePage_WhenSortedSetRangeByRankAsyncThrowsException_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cursor = "cursor";
        var count = 5;

        _redis.SortedSetRank(userId.ToString(), cursor).Returns(0);
        _redis
            .SortedSetRangeByRankAsync($"timeline:{userId}", 1, count + 1, Order.Descending)
            .Returns(Task.FromException<RedisValue[]>(new Exception()));

        // Act
        var postIds = await _timelineService.GetTimelinePage(userId, cursor, count);

        // Assert
        postIds.Should().BeEmpty();
    }
}
