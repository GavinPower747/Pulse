namespace Pulse.Posts.UI.Models;

public class PostViewModel
{
    public Guid Id { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? DisplayName { get; set; }
    public string? Username { get; set; }
    public string? AvatarUrl { get; set; }
    public IEnumerable<string> AttachmentUrls { get; set; } = ["https://dummyimage.com/1280x720/fff/aaa"];

    public string GetUserInitials() =>
        DisplayName?.Split(' ').Select(x => x[0]).Aggregate(string.Empty, (x, y) => x + y)
        ?? string.Empty;
}
