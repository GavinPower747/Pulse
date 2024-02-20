namespace Pulse.Timeline.Contracts.Commands;

public class AddPostToTimelineCommand(Guid userId, Guid postId, DateTimeOffset created)
{
    public Guid UserId { get; set; } = userId;
    public Guid PostId { get; set; } = postId;
    public DateTimeOffset Created { get; set; } = created;
}
