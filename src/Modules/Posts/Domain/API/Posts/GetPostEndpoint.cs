using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Posts.Contracts;
using Pulse.Posts.UI.Components;

namespace Pulse.Posts.API.Posts;

public class GetPostEndpoint(IPostQueryService postQueryService)
{
    private readonly IPostQueryService _postQueryService = postQueryService;

    public async Task<IResult> Handle(Guid postId, CancellationToken cancellationToken)
    {
        var post = await _postQueryService.Get(postId, cancellationToken);

        if(post is null)
            return TypedResults.NotFound();

        return new RazorComponentResult<Post>(new { CurrentPost = post });
    }
}
