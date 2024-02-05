namespace Pulse.WebApp.Features.Posts.API.Create
{
    public record CreatePostRequest
    {
        public required string Content { get; set; }
    }
}