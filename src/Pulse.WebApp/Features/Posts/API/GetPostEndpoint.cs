using Microsoft.AspNetCore.Mvc;

using Pulse.Posts.Contracts;

namespace Pulse.WebApp.Features.Posts.API;

public class GetPostEndpoint(IPostQueryService postQueryService)
{
    private readonly IPostQueryService _postQueryService = postQueryService;

    public async Task<IResult> Handle(Guid postId, CancellationToken cancellationToken)
    {
        var post = await _postQueryService.Get(postId, cancellationToken);

        if (post is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(post);
    }
}