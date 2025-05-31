using Pulse.Shared.Messaging;

namespace Pulse.Followers.Contracts.Events;

public class UserUnfollowedEvent(Guid userId, Guid followerId) : IntegrationEvent
{
    public override string EventName => "followers.userUnfollowed"; 
    public override string EventVersion => "v1";
    public override Uri Source => new("pulse://followers");


    public Guid UserId { get; set; } = userId;
    public Guid FollowerId { get; set; } = followerId;
}
