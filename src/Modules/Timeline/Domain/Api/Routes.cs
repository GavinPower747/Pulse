using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Pulse.WebApp.Features.Timeline.Api;

namespace Pulse.Timeline;

public static class Routes
{
    public static RouteGroupBuilder MapTimelineRoutes(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/timeline").RequireAuthorization();

        group.WithTags("Timeline");

        group.MapGet(
            "/",
            async (
                GetTimelinePageRequest request,
                [FromServices] GetTimelinePageEndpoint handler,
                CancellationToken cancellationToken
            ) => await handler.Handle(request, cancellationToken)
        );

        group.MapGet(
            "/updates",
            async (
                [FromHeader(Name = "If-None-Match")] string etag,
                [FromServices] GetTimelineUpdatesEndpoint handler,
                CancellationToken cancellationToken
            ) => await handler.Handle(etag, cancellationToken)
        );

        return group;
    }
}
