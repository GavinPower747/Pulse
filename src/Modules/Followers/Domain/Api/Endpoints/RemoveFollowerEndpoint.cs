using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Followers.Contracts.Services;
using Pulse.Followers.UI.Components;
using Pulse.Shared.Auth;

namespace Pulse.Followers;

public class RemoveFollowerEndpoint(
    IdentityProvider identityProvider,
    IFollowerProvider followerProvider
)
{
    private readonly IdentityProvider _identityProvider = identityProvider;
    private readonly IFollowerProvider _followerProvider = followerProvider;

    public async Task<IResult> Handle(Guid followingId, CancellationToken cancellationToken)
    {
        var userId = _identityProvider.GetCurrentUser().Id;

        await _followerProvider.Unfollow(userId, followingId, cancellationToken);

        var parameters = new Dictionary<string, object?>()
        {
            { nameof(FollowButton.UserId), followingId },
            { nameof(FollowButton.IsFollowing), false }
        };

        return new RazorComponentResult<FollowButton>(parameters);
    }
}
