using Pulse.Shared.Messaging;

namespace Pulse.Timeline.Contracts.Commands;

public class AddPostToTimelineCommand(Guid userId, Guid postId, DateTimeOffset created) : IntegrationEvent
{
    public override string EventName => nameof(AddPostToTimelineCommand);
    public override string EventVersion => "v1";
    public override Uri Source => new("pulse://timeline");

    public Guid UserId { get; set; } = userId;
    public Guid PostId { get; set; } = postId;
    public DateTimeOffset Created { get; set; } = created;
}
