namespace Pulse.WebApp.Features.Posts.Models;

public class PostViewModel
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? DisplayName { get; set; }
    public string? Username { get; set; }
    public string? AvatarUrl { get; set; }
}
