using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

using Pulse.Posts.Contracts;
using Pulse.Posts.Domain;
using Pulse.Posts.Domain.Mapping;
using Pulse.Posts.Services;
using Pulse.Posts.UI.Components;

namespace Pulse.Posts.API.Attachments;

internal class UploadAttachmentEndpoint(AttachmentService attachmentService)
{
    private readonly AttachmentService _attachmentService = attachmentService;

    public async Task<IResult> Handle(IFormFile file, Guid? postId, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest("File is required");

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("File size must be less than 10MB");

        if (file.ContentType != "image/jpeg" && file.ContentType != "image/png")
            return BadRequest("File must be a JPEG or PNG image");

        var fileStream = file.OpenReadStream();
        using var attachment = new Attachment(new AttachmentMetadata(Guid.NewGuid(), postId ?? Guid.NewGuid(), AttachmentType.Image, file.Length, file.ContentType, string.Empty), fileStream);

        await _attachmentService.Upload(attachment, ct);
        var attachments = await _attachmentService.GetPostAttachmentMetadata(attachment.Metadata.PostId, ct);
        var downloadDetails = attachments.Select(DomainDtoMapper.MapToAttachmentDownload);

        return Ok(attachment.Metadata.PostId, downloadDetails);
    }

    private static RazorComponentResult<PostForm> BadRequest(string message)
    {
        var componentParams = new Dictionary<string, object?>
        {
            { nameof(PostForm.ErrorMessage), message }
        };

        var result = new RazorComponentResult<PostForm>(componentParams)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        return result;
    }

    private static RazorComponentResult<PostForm> Ok(Guid postId, IEnumerable<AttachmentDownload> attachments)
    {
        var componentParams = new Dictionary<string, object?>
        {
            { nameof(PostForm.PostId), postId },
            { nameof(PostForm.Attachments), attachments }
        };

        var result = new RazorComponentResult<PostForm>(componentParams)
        {
            StatusCode = StatusCodes.Status200OK
        };

        return result;
    }
}
