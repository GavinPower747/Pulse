using Pulse.Shared.Messaging;

namespace Pulse.Followers.Contracts.Events;

public class UserFollowedEvent(Guid followerId, Guid followingId) : IntegrationEvent
{
    public override string EventName => "followers.userFollowed";
    public override string EventVersion => "v1";
    public override Uri Source => new("https://followers.pulse");

    public Guid FollowerId { get; set; } = followerId;
    public Guid FollowingId { get; set; } = followingId;
}
