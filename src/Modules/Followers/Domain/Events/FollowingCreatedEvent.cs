using Pulse.Shared.Domain;

namespace Pulse.Followers.Events;

public class FollowingCreatedEvent(Guid followerId, Guid followingId) : IDomainEvent
{
    public Guid FollowerId { get; } = followerId;
    public Guid FollowingId { get; } = followingId;
}
