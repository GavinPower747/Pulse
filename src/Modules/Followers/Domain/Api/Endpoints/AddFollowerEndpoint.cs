using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Followers.Contracts.Services;
using Pulse.Followers.UI.Components;
using Pulse.Shared.Auth;

namespace Pulse.Followers.Api.Endpoints;

internal class AddFollowerEndpoint(
    IdentityProvider identityService,
    IFollowerProvider followerProvider
)
{
    private readonly IdentityProvider _identityService = identityService;
    private readonly IFollowerProvider _followerProvider = followerProvider;

    public async Task<IResult> Handle(Guid followingId, CancellationToken cancellationToken)
    {
        var userId = _identityService.GetCurrentUser().Id;

        await _followerProvider.Follow(userId, followingId, cancellationToken);

        var parameters = new Dictionary<string, object?>()
        {
            { nameof(FollowButton.UserId), followingId },
            { nameof(FollowButton.IsFollowing), true }
        };

        return new RazorComponentResult<FollowButton>(parameters);
    }
}
