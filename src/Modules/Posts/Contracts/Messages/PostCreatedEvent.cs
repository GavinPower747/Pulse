using Pulse.Shared.Messaging;

namespace Pulse.Posts.Contracts.Messages;

public class PostCreatedEvent(Guid id, Guid userId, DateTimeOffset created) : IntegrationEvent
{
    public override string EventName => "posts.postCreated";
    public override string EventVersion => "v1";
    public override Uri Source => new("https://posts.pulse");

    public Guid Id { get; set; } = id;
    public Guid UserId { get; set; } = userId;
    public DateTimeOffset Created { get; set; } = created;
}
