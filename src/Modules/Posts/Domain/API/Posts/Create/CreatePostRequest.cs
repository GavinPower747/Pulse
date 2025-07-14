namespace Pulse.Posts.API.Posts.Create;

public record CreatePostRequest
{
    public required string Content { get; set; }
}
