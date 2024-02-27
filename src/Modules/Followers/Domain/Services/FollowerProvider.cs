using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pulse.Followers.Contracts.Services;
using Pulse.Followers.Data;

namespace Pulse.Followers.Domain.Services;

internal class FollowerProvider(FollowingContext dbContext, ILoggerFactory loggerFactory)
    : IFollowerProvider
{
    private readonly FollowingContext _dbContext = dbContext;
    private readonly ILogger<FollowerProvider> _logger =
        loggerFactory.CreateLogger<FollowerProvider>();

    public async Task Follow(Guid userId, Guid followerId, CancellationToken cancellationToken)
    {
        try
        {
            var following = new Following(userId, followerId);

            _dbContext.Followings.Add(following);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            //If we're trying to follow someone we're already following, we don't want to throw an exception
            _logger.LogWarning(
                "User {UserId} is already following user {FollowerId} but tried to follow them again",
                userId,
                followerId
            );
        }
    }

    public async Task Unfollow(Guid userId, Guid followerId, CancellationToken cancellationToken)
    {
        var following = await _dbContext.Followings.FindAsync(
            userId,
            followerId,
            cancellationToken
        );

        if (following is null)
        {
            _logger.LogWarning(
                "User {UserId} is not following user {FollowerId} but tried to unfollow them",
                userId,
                followerId
            );

            return;
        }

        _dbContext.Followings.Remove(following);
        following.AddDomainEvent(new FollowingDeletedEvent(userId, followerId));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Guid>> GetFollowers(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var followers = await _dbContext
            .Followings.Where(f => f.FollowingId == userId)
            .Select(f => f.UserId)
            .ToListAsync(cancellationToken);

        return followers;
    }

    public async Task<IEnumerable<Guid>> GetFollowing(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var following = await _dbContext
            .Followings.Where(f => f.UserId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync(cancellationToken);

        return following;
    }

    public async Task<bool> IsFollowing(
        Guid userId,
        Guid followerId,
        CancellationToken cancellationToken
    )
    {
        var following = await _dbContext.Followings.FindAsync(
            [userId, followerId],
            cancellationToken: cancellationToken
        );

        return following is not null;
    }
}
