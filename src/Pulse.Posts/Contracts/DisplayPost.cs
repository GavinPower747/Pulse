namespace Pulse.Posts.Contracts;

public record DisplayPost(
    Guid Id,
    Guid UserId,
    string Content,
    DateTime CreatedAt,
    DateTime? ScheduledAt,
    DateTime? PublishedAt
)
{
    public Guid Id { get; init; } = Id;
    public Guid UserId { get; init; } = UserId;
    public string PostContent { get; init; } = Content;
    public DateTime CreatedAt { get; init; } = CreatedAt;
    public DateTime? ScheduledAt { get; init; } = ScheduledAt;
    public DateTime? PublishedAt { get; init; } = PublishedAt;
}
