using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Posts.Contracts;
using Pulse.Posts.UI.Mapping;
using Pulse.Shared.Auth;
using Pulse.Timeline.Contracts;
using Pulse.Timeline.UI.Components;
using Pulse.Users.Contracts;

namespace Pulse.Timeline.Api;

public class GetTimelinePageEndpoint(
    ITimelineService timelineService,
    IPostQueryService postQuery,
    IUserQueries userQuery,
    PostMapper mapper,
    IdentityProvider identityProvider
)
{
    public async Task<IResult> Handle(
        GetTimelinePageRequest request,
        CancellationToken cancellationToken
    )
    {
        var currentUser = identityProvider.GetCurrentUser();

        var timelinePage = await timelineService.GetTimelinePage(
            currentUser.Id,
            request.ContinuationToken ?? string.Empty,
            request.PageSize,
            cancellationToken
        );

        var posts = await postQuery.Get(timelinePage.Ids, cancellationToken);
        var userTasks = posts.Select(p => p.UserId).Distinct().Select(userQuery.GetUser);

        var users = await Task.WhenAll(userTasks);
        var viewModels = posts.Select(p =>
        {
            var user = users.First(u => u.Id == p.UserId);
            return mapper.MapToViewModel(p, user);
        });

        var viewArgs = new Dictionary<string, object?>
        {
            [nameof(FeedPage.EagerPosts)] = viewModels.ToList(),
            [nameof(FeedPage.ContinuationToken)] = timelinePage.ContinuationToken,
        };

        return new RazorComponentResult<FeedPage>(viewArgs);
    }
}
