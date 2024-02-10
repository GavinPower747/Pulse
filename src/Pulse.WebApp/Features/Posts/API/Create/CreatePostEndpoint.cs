using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Posts.Contracts;
using Pulse.WebApp.Auth;
using Pulse.WebApp.Features.Posts.API.Create;
using Pulse.WebApp.Features.Posts.Components;
using Pulse.WebApp.Features.Posts.Mapping;

namespace Pulse.WebApp.Features.Posts.API;

public class CreatePostEndpoint(
    IPostCreator postCreator,
    PostMapper mapper,
    IdentityProvider identityProvider
)
{
    private readonly IPostCreator _postCreator = postCreator;
    private readonly PostMapper _mapper = mapper;
    private readonly IdentityProvider _identityProvider = identityProvider;

    public async Task<IResult> Handle(CreatePostRequest request)
    {
        var currentUser = _identityProvider.GetCurrentUser();

        var post = await _postCreator.Create(currentUser.Id, request.Content);
        var postModel = _mapper.MapToViewModel(post, currentUser.UserName, currentUser.DisplayName);

        var componentParams = new Dictionary<string, object?>
        {
            { nameof(Post.CurrentPost), postModel }
        };

        return new RazorComponentResult<Post>(componentParams);
    }
}
