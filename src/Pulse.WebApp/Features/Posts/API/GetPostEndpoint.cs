using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Posts.Contracts;
using Pulse.WebApp.Features.Posts.Components;

namespace Pulse.WebApp.Features.Posts.API;

public class GetPostEndpoint(IPostQueryService postQueryService)
{
    private readonly IPostQueryService _postQueryService = postQueryService;

    public async Task<IResult> Handle(Guid postId, CancellationToken cancellationToken)
    {
        var post = await _postQueryService.Get(postId, cancellationToken);

        return post is not null
            ? new RazorComponentResult<Post>(new { CurrentPost = post })
            : TypedResults.NotFound();
    }
}
