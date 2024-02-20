namespace Pulse.Posts.Domain;

internal class Post
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Post(
        Guid id,
        Guid userId,
        string content,
        DateTime createdAt,
        DateTime? scheduledAt,
        DateTime? publishedAt
    )
    {
        Id = id;
        UserId = userId;
        Content = content;
        CreatedAt = createdAt;
        ScheduledAt = scheduledAt;
        PublishedAt = publishedAt;
    }

    // For unit tests *sigh*
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Post() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
