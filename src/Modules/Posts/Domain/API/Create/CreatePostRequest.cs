namespace Pulse.Posts.API.Create;

public record CreatePostRequest
{
    public required string Content { get; set; }
}
