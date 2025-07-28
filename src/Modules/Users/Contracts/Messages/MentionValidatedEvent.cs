using Pulse.Shared.Messaging;

namespace Pulse.Users.Contracts.Messages;

public class MentionValidatedEvent(Guid postId, string username) : IntegrationEvent
{
    public override string EventName => "user.mentionValidated";
    public override string EventVersion => "v1";
    public override Uri Source => new("https://user.pulse");

    public Guid PostId { get; set; } = postId;
    public string Username { get; set; } = username;
}