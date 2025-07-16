namespace Pulse.Posts.API.Posts.Create;

public record CreatePostRequest
{
    public Guid? PostId { get; set; }
    public required string Content { get; set; }
}
