using Microsoft.AspNetCore.Mvc;

using Pulse.WebApp.Features.Posts.API.Create;

namespace Pulse.WebApp.Features.Posts.API;

internal static class Routes
{
    public static RouteGroupBuilder MapPostRoutes(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/post");

        group.WithTags("Post");

        group.MapPost("/", 
            async ([FromForm]CreatePostRequest request, [FromServices] CreatePostEndpoint handler ) 
                => await handler.Handle(request));
        
        return group;
    }
}