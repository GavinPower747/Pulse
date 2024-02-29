using MediatR;

namespace Pulse.Followers.Events;

internal class FollowingCreatedEvent(Guid followerId, Guid followingId) : INotification
{
    public Guid FollowerId { get; } = followerId;
    public Guid FollowingId { get; } = followingId;
}
