using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Pulse.Posts.API;
using Pulse.Posts.API.Create;

namespace Pulse.Posts;

public static class Routes
{
    public static RouteGroupBuilder MapPostRoutes(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/post").RequireAuthorization();

        group.WithTags("Post");

        group.MapPost(
            "/",
            async (
                [FromForm] CreatePostRequest request,
                [FromServices] CreatePostEndpoint handler
            ) => await handler.Handle(request)
        );

        group.MapGet(
            "/{postId}",
            async (
                Guid postId,
                [FromServices] GetPostEndpoint handler,
                CancellationToken cancellationToken
            ) => await handler.Handle(postId, cancellationToken)
        );

        return group;
    }
}

internal static class PostApiExtensions
{
    public static ContainerBuilder AddPostEndpoints(this ContainerBuilder builder)
    {
        builder.RegisterType<CreatePostEndpoint>().AsSelf().SingleInstance();
        builder.RegisterType<GetPostEndpoint>().AsSelf().SingleInstance();

        return builder;
    }
}
