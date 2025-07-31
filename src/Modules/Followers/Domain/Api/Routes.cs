using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Pulse.Followers.Api.Endpoints;

namespace Pulse.Followers;

public static class Routes
{
    public static RouteGroupBuilder MapFollowerRoutes(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/user/{userId}/followers").RequireAuthorization();

        group.WithTags("Followers");

        group.MapPost(
            "/",
            async (
                [FromRoute] Guid userId,
                [FromServices] AddFollowerEndpoint handler,
                CancellationToken cancellationToken
            ) => await handler.Handle(userId, cancellationToken)
        );

        group.MapDelete(
            "/",
            async (
                [FromRoute] Guid userId,
                [FromServices] RemoveFollowerEndpoint handler,
                CancellationToken cancellationToken
            ) => await handler.Handle(userId, cancellationToken)
        );

        return group;
    }
}
