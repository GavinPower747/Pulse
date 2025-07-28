using Pulse.Shared.Messaging;

namespace Pulse.Posts.Contracts.Messages;

public class UserMentionedEvent(Guid postId, string username) : IntegrationEvent
{
    public override string EventName => "posts.userMentioned";
    public override string EventVersion => "v1";
    public override Uri Source => new("https://posts.pulse");

    public Guid PostId { get; set; } = postId;
    public string Username { get; set; } = username;
}