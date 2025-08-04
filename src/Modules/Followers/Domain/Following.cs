using Pulse.Followers.Events;
using Pulse.Shared;

namespace Pulse.Followers.Domain;

/// <summary>
/// A domain object that represents a following relationship between two users.
/// </summary>
internal class Following : Entity
{
    /// <summary>
    /// The user who created the following relationship
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The user who is being followed by the above user
    /// </summary>
    public Guid FollowingId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Following(Guid userId, Guid followingId, DateTime createdAt)
    {
        UserId = userId;
        FollowingId = followingId;
        CreatedAt = createdAt;
    }

    public Following(Guid userId, Guid followingId)
    {
        UserId = userId;
        FollowingId = followingId;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new FollowingCreatedEvent(UserId, FollowingId));
    }
}
