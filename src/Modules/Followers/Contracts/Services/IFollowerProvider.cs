namespace Pulse.Followers.Contracts.Services;

public interface IFollowerProvider
{
    Task<IEnumerable<Guid>> GetFollowers(Guid userId, CancellationToken cancellationToken);
    Task<IEnumerable<Guid>> GetFollowing(Guid userId, CancellationToken cancellationToken);
    Task<int> GetFollowerCount(Guid userId, CancellationToken cancellationToken);
    Task<int> GetFollowingCount(Guid userId, CancellationToken cancellationToken);
    Task<bool> IsFollowing(Guid userId, Guid followerId, CancellationToken cancellationToken);
    Task Follow(Guid userId, Guid followerId, CancellationToken cancellationToken);
    Task Unfollow(Guid userId, Guid followerId, CancellationToken cancellationToken);
}
