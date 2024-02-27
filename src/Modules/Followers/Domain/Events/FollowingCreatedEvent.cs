using MediatR;

namespace Pulse.Followers.Events;

public class FollowingCreatedEvent(Guid followerId, Guid followingId) : INotification
{
    public Guid FollowerId { get; } = followerId;
    public Guid FollowingId { get; } = followingId;
}
