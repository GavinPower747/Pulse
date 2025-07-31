using Pulse.Shared.Domain;

namespace Pulse.Followers;

internal class FollowingDeletedEvent(Guid userId, Guid followerId) : IDomainEvent
{
    public Guid UserId { get; } = userId;
    public Guid FollowerId { get; } = followerId;
}
