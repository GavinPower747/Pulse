using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Pulse.Posts.Services;

namespace Pulse.Posts.API.Attachments;

internal class GetAttachmentEndpoint(AttachmentService attachmentService)
{
    private readonly AttachmentService _attachmentService = attachmentService;

    public async Task<IResult> Handle(string fileName, CancellationToken ct)
    {
        var attachmentIdString = Path.GetFileNameWithoutExtension(fileName);

        if (!Guid.TryParse(attachmentIdString, out var attachmentId))
            return Results.NotFound();

        var attachment = await _attachmentService.GetAttachment(attachmentId, ct);

        if (attachment == null)
            return Results.NotFound();

        return Results.File(
            attachment.Content,
            attachment.Metadata.ContentType,
            attachment.Metadata.GetFileName(),
            null,
            !string.IsNullOrWhiteSpace(attachment.Metadata.ETag)
                ? new EntityTagHeaderValue(attachment.Metadata.ETag)
                : null,
            false
        );
    }
}
