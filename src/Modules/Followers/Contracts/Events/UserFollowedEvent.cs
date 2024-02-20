namespace Pulse.Followers.Contracts.Events;

public class UserFollowedEvent(Guid followerId, Guid followingId)
{
    public Guid FollowerId { get; set; } = followerId;
    public Guid FollowingId { get; set; } = followingId;
}
