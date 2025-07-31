using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Posts.Contracts;
using Pulse.Posts.Services;
using Pulse.Posts.UI.Components;

namespace Pulse.Posts.API.Attachments;

internal class DeleteAttachmentEndpoint(AttachmentService attachmentService)
{
    private readonly AttachmentService _attachmentService = attachmentService;

    public async Task<IResult> Handle(Guid attachmentId, Guid postId, CancellationToken ct)
    {
        await _attachmentService.DeleteAttachment(attachmentId, ct);
        var attachments = await _attachmentService.GetPostAttachmentMetadata(postId, ct);
        var downloadDetails = attachments.Select(a => new AttachmentDownload(
            a.Id,
            Routes.GetAttachment(a.PostId, a.GetFileName()),
            a.ETag
        ));

        return Ok(postId, downloadDetails);
    }

    private static RazorComponentResult<PostForm> Ok(
        Guid postId,
        IEnumerable<AttachmentDownload> attachments
    )
    {
        var componentParams = new Dictionary<string, object?>
        {
            { nameof(PostForm.PostId), postId },
            { nameof(PostForm.Attachments), attachments },
        };

        var result = new RazorComponentResult<PostForm>(componentParams)
        {
            StatusCode = StatusCodes.Status200OK,
        };

        return result;
    }
}
