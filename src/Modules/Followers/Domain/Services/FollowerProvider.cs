using Pulse.Followers.Contracts.Services;

namespace Pulse.Followers.Domain.Services;

internal class FollowerProvider : IFollowerProvider
{
    public Task Follow(Guid userId, Guid followerId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Guid>> GetFollowers(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Guid>> GetFollowing(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsFollowing(Guid userId, Guid followerId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Unfollow(Guid userId, Guid followerId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
