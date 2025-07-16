using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Pulse.Posts.Domain;
using Pulse.Posts.Services;
using Pulse.Posts.UI.Components;

namespace Pulse.Posts.API.Attachments.Create;

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
        Span<byte> magicNumber = stackalloc byte[4];
        fileStream.ReadExactly(magicNumber);

        switch(file.ContentType)
        {
            case "image/jpeg":
                if (magicNumber[0] == 0xFF && magicNumber[1] == 0xD8 && magicNumber[2] == 0xFF)
                    return BadRequest("File was not a valid JPEG image");
                break;
            case "image/png":
                if (magicNumber[0] == 0x89 && magicNumber[1] == 0x50 && magicNumber[2] == 0x4E && magicNumber[3] == 0x47)
                    return BadRequest("File was not a valid PNG image");
                break;
            default:
                return BadRequest("File must be a JPEG or PNG image");
        }

        using var attachment = new Attachment(new AttachmentMetadata(Guid.NewGuid(), postId ?? Guid.NewGuid(), AttachmentType.Image, file.Length, file.ContentType), fileStream);

        await _attachmentService.Upload(attachment, ct);
        var attachments = await _attachmentService.GetAttachments(attachment.Metadata.PostId, ct);

        return Ok(attachment.Metadata.PostId, attachments);
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

    private static RazorComponentResult<PostForm> Ok(Guid postId, IEnumerable<AttachmentMetadata> attachments)
    {
        var componentParams = new Dictionary<string, object?>
        {
            { nameof(PostForm.PostId), postId },
            { nameof(PostForm.AttachmentUrls), attachments.Select(a => a.FileKey) }
        };

        var result = new RazorComponentResult<PostForm>(componentParams)
        {
            StatusCode = StatusCodes.Status200OK
        };

        return result;
    }
}
