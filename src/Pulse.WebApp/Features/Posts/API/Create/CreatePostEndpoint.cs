using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Pulse.Posts.Contracts;
using Pulse.WebApp.Auth;
using Pulse.WebApp.Features.Posts.API.Create;
using Pulse.WebApp.Features.Posts.Components;

namespace Pulse.WebApp.Features.Posts.API;

public class CreatePostEndpoint(IPostCreator postCreator, IHttpContextAccessor httpContextAccessor)
{
    private readonly IPostCreator _postCreator = postCreator;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<IResult> Handle(CreatePostRequest request)
    {
        var userId = _httpContextAccessor.HttpContext?.GetUserId();

        var post = await _postCreator.Create(userId.GetValueOrDefault(), request.Content);

        var componentParams = new Dictionary<string, object?>
        {
            { nameof(Post.CurrentPost), post }
        };

        return new RazorComponentResult<Post>(componentParams);
    }
}