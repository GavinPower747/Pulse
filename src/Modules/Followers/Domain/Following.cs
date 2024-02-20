using Pulse.Followers.Events;
using Pulse.Shared.Domain;

namespace Pulse.Followers.Domain;

/// <summary>
/// A domain object that represents a following relationship between two users.
/// </summary>
internal class Following
{
    /// <summary>
    /// The Id of the relationship
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The user who created the following relationship
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The user who is being followed by the above user
    /// </summary>
    public Guid FollowingId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    private readonly List<IDomainEvent> _domainEvents = [];

    public Following(Guid id, Guid userId, Guid followingId, DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        FollowingId = followingId;
        CreatedAt = createdAt;
    }

    public Following(Guid userId, Guid followingId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        FollowingId = followingId;
        CreatedAt = DateTime.UtcNow;

        _domainEvents.Add(new FollowingCreatedEvent(UserId, FollowingId));
    }
}
