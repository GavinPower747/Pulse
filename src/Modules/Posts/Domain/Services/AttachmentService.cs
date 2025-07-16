using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Data;
using Pulse.Posts.Domain;

namespace Pulse.Posts.Services;

internal class AttachmentService(IAmazonS3 s3Client, AttachmentContext attachmentContext)
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly AttachmentContext _attachmentContext = attachmentContext;

    public async Task Upload(Attachment attachment, CancellationToken ct)
    {
        var tx = await _attachmentContext.Database.BeginTransactionAsync(ct);

        _attachmentContext.AttachmentMetadata.Add(attachment.Metadata);
        await _attachmentContext.SaveChangesAsync(ct);

        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = "pulse-attachments",
            Key = attachment.Metadata.FileKey,
            InputStream = attachment.Content,
            ContentType = attachment.Metadata.ContentType
        }, ct);

        await tx.CommitAsync(ct);
    }

    public async Task<IEnumerable<AttachmentMetadata>> GetAttachments(Guid postId, CancellationToken ct)
    {
        return await _attachmentContext.AttachmentMetadata.Where(a => a.PostId == postId).ToListAsync(ct);
    }
}