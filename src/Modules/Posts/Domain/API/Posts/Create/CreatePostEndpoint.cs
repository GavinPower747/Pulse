using System.Web;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Posts.Contracts;
using Pulse.Posts.UI.Mapping;
using Pulse.Shared.Auth;

namespace Pulse.Posts.API.Posts.Create;

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
        if (request.Content.Length > 280)
            return BadRequest("Post content must be 280 characters or less.");
        
        var processedPostContent = ProcessContent(request.Content);

        var currentUser = _identityProvider.GetCurrentUser();
        var post = await _postCreator.Create(request.PostId, currentUser.Id, processedPostContent);

        return Ok(post, currentUser);
    }

    // Remove HTML to prevent XSS and CSS attacks but preserve people trying to share code snippets
    // Also replace new lines with <br/> to maintain formatting
    private static string ProcessContent(string content)
    {
        var encodedContent = HttpUtility.HtmlEncode(content);

        encodedContent = encodedContent
                            .Replace("\r\n", "<br/>")
                            .Replace("\n", "<br/>")
                            .Replace("\r", "<br/>");

        return encodedContent;
    }

    private static RazorComponentResult<CreatePostResponse> BadRequest(string message)
    {
        var componentParams = new Dictionary<string, object?>
        {
            { nameof(CreatePostResponse.ErrorMessage), message }
        };

        var result = new RazorComponentResult<CreatePostResponse>(componentParams)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        return result;
    }

    private RazorComponentResult<CreatePostResponse> Ok(
        DisplayPost addedPost,
        CurrentUser currentUser
    )
    {
        var postVm = _mapper.MapToViewModel(
            addedPost,
            currentUser.UserName,
            currentUser.DisplayName
        );

        var componentParams = new Dictionary<string, object?>
        {
            { nameof(CreatePostResponse.AddedPost), postVm }
        };

        return new RazorComponentResult<CreatePostResponse>(componentParams);
    }
}
