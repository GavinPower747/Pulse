namespace Pulse.Followers.Contracts.Events;

public class UserUnfollowedEvent(Guid userId, Guid followerId)
{
    public Guid UserId { get; set; } = userId;
    public Guid FollowerId { get; set; } = followerId;
}
