using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Posts.Contracts;
using Pulse.WebApp.Auth;
using Pulse.WebApp.Features.Posts.API.Create;
using Pulse.WebApp.Features.Posts.Components;
using Pulse.WebApp.Features.Posts.Mapping;

namespace Pulse.WebApp.Features.Posts.API;

public class CreatePostEndpoint(
    IPostCreator postCreator,
    IHttpContextAccessor httpContextAccessor,
    PostMapper mapper
)
{
    private readonly IPostCreator _postCreator = postCreator;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly PostMapper _mapper = mapper;

    public async Task<IResult> Handle(CreatePostRequest request)
    {
        var userId = _httpContextAccessor.HttpContext?.GetUserId();
        var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        var displayName =
            _httpContextAccessor.HttpContext?.User.FindFirst("name")?.Value ?? "Anonymous";

        var post = await _postCreator.Create(userId.GetValueOrDefault(), request.Content);
        var postModel = _mapper.MapToViewModel(post, userName, displayName);

        var componentParams = new Dictionary<string, object?>
        {
            { nameof(Post.CurrentPost), postModel }
        };

        return new RazorComponentResult<Post>(componentParams);
    }
}
