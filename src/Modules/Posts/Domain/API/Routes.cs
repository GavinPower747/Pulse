using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Pulse.Posts.API.Attachments;

using Pulse.Posts.API.Posts;
using Pulse.Posts.API.Posts.Create;

namespace Pulse.Posts;

public static class Routes
{
    internal static string GetAttachment(Guid postId, string fileName) => $"/api/post/{postId}/attachment/{fileName}";

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
                CancellationToken ct
            ) => await handler.Handle(postId, ct)
        );

        group.MapPost(
            "/attachment",
            async (
                [FromForm(Name = "attachments")] IFormFile file,
                [FromServices] UploadAttachmentEndpoint handler,
                CancellationToken ct
            ) => await handler.Handle(file, null, ct)
        );

        group.MapPost(
            "{postId}/attachment/",
            async (
                Guid postId,
                [FromForm(Name = "attachments")] IFormFile file,
                [FromServices] UploadAttachmentEndpoint handler,
                CancellationToken ct
            ) => await handler.Handle(file, postId, ct)
        );

        group.MapGet(
            "{postId}/attachment/{fileName}",
            async (
                Guid postId,
                string fileName,
                [FromServices] GetAttachmentEndpoint handler,
                CancellationToken ct
            ) => await handler.Handle(fileName, ct)
        );

        group.MapDelete(
            "{postId}/attachment/{attachmentId}",
            async (
                Guid postId,
                Guid attachmentId,
                [FromServices] DeleteAttachmentEndpoint handler,
                CancellationToken ct
            ) => await handler.Handle(attachmentId, postId, ct)
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
        builder.RegisterType<GetAttachmentEndpoint>().AsSelf().SingleInstance();
        builder.RegisterType<UploadAttachmentEndpoint>().AsSelf().SingleInstance();
        builder.RegisterType<DeleteAttachmentEndpoint>().AsSelf().SingleInstance();

        return builder;
    }
}
