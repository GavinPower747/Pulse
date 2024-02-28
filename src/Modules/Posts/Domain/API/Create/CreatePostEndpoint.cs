using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Posts.Contracts;
using Pulse.Posts.UI.Components;
using Pulse.Posts.UI.Mapping;
using Pulse.Shared.Auth;
using Pulse.WebApp.Features.Posts.API.Create;

namespace Pulse.WebApp.Features.Posts.API;

internal class CreatePostEndpoint(
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
