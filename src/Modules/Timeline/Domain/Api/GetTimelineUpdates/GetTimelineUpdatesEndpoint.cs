using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Shared.Auth;
using Pulse.Timeline.Contracts;
using Pulse.Timeline.UI.Components;

namespace Pulse.Timeline;

public class GetTimelineUpdatesEndpoint(
    ITimelineService timelineService,
    IdentityProvider identityProvider
)
{
    private readonly ITimelineService _timelineService = timelineService;
    private readonly IdentityProvider _identityProvider = identityProvider;

    public async Task<IResult> Handle(string etag, CancellationToken cancellationToken)
    {
        var userId = _identityProvider.GetCurrentUser().Id;
        var (hasChanges, newEtag) = await _timelineService.CheckForChanges(
            userId,
            etag,
            cancellationToken
        );

        var viewProperties = new Dictionary<string, object?>
        {
            { nameof(LoadMoreButton.Etag), newEtag },
            { nameof(LoadMoreButton.HasChanges), hasChanges },
        };

        return new RazorComponentResult<LoadMoreButton>(viewProperties);
    }
}
