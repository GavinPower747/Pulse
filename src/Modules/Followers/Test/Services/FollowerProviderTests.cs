using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pulse.Followers.Data;
using Pulse.Followers.Domain;
using Pulse.Followers.Domain.Services;

namespace Pulse.Followers.Test;

public class FollowerProviderTests
{
    private readonly FollowingContext _dbContext;
    private readonly ILoggerFactory _loggerFactory;
    private readonly FollowerProvider _followerProvider;

    public FollowerProviderTests()
    {
        var options = new DbContextOptionsBuilder<FollowingContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        var mediator = Substitute.For<IMediator>();
        _dbContext = new FollowingContext(options, mediator);
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _followerProvider = new FollowerProvider(_dbContext, _loggerFactory);
    }

    [Fact]
    public async Task Follow_ShouldAddFollowingToDbContext()
    {
        var userId = Guid.NewGuid();
        var followerId = Guid.NewGuid();

        await _followerProvider.Follow(userId, followerId, default);

        Assert.NotEmpty(_dbContext.Followings);
    }

    [Fact]
    public async Task Unfollow_ShouldRemoveFollowingFromDbContext()
    {
        var userId = Guid.NewGuid();
        var followerId = Guid.NewGuid();

        _dbContext.Followings.Add(new Following(userId, followerId));
        await _dbContext.SaveChangesAsync();

        await _followerProvider.Unfollow(userId, followerId, default);

        Assert.Empty(_dbContext.Followings);
    }

    [Fact]
    public async Task GetFollowers_ShouldReturnFollowersFromDbContext()
    {
        var userId = Guid.NewGuid();
        var followerId = Guid.NewGuid();

        _dbContext.Followings.Add(new Following(followerId, userId));
        await _dbContext.SaveChangesAsync();

        var result = await _followerProvider.GetFollowers(userId, default);

        Assert.Contains(followerId, result);
    }

    [Fact]
    public async Task GetFollowing_ShouldReturnFollowingFromDbContext()
    {
        var userId = Guid.NewGuid();
        var followingId = Guid.NewGuid();

        _dbContext.Followings.Add(new Following(userId, followingId));
        await _dbContext.SaveChangesAsync();

        var result = await _followerProvider.GetFollowing(userId, default);

        Assert.Contains(followingId, result);
    }

    [Fact]
    public async Task IsFollowing_ShouldReturnTrueIfFollowingExistsInDbContext()
    {
        var userId = Guid.NewGuid();
        var followingId = Guid.NewGuid();

        _dbContext.Followings.Add(new Following(userId, followingId));
        await _dbContext.SaveChangesAsync();

        var result = await _followerProvider.IsFollowing(userId, followingId, default);

        Assert.True(result);
    }

    [Fact]
    public async Task IsFollowing_ShouldReturnFalseIfFollowingDoesNotExistInDbContext()
    {
        var userId = Guid.NewGuid();
        var followingId = Guid.NewGuid();

        var result = await _followerProvider.IsFollowing(userId, followingId, default);

        Assert.False(result);
    }
}
