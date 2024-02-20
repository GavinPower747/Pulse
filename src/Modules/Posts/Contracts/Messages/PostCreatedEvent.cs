namespace Pulse.Posts.Contracts.Messages;

public class PostCreatedEvent(Guid id, Guid userId, DateTimeOffset created)
{
    public Guid Id { get; set; } = id;
    public Guid UserId { get; set; } = userId;
    public DateTimeOffset Created { get; set; } = created;
}
