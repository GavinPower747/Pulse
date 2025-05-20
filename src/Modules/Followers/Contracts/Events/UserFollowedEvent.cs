using Pulse.Shared.Messaging;

namespace Pulse.Followers.Contracts.Events;

public class UserFollowedEvent(Guid followerId, Guid followingId) : IntegrationEvent
{
    public override string EventName => nameof(UserFollowedEvent);
    public override string EventVersion => "v1";
    public override Uri Source => new("pulse://followers");

    public Guid FollowerId { get; set; } = followerId;
    public Guid FollowingId { get; set; } = followingId;
}
