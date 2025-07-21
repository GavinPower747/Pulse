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
    private const string BucketName = "pulse-attachments";

    public async Task Upload(Attachment attachment, CancellationToken ct)
    {
        using var tx = await _attachmentContext.Database.BeginTransactionAsync(ct);

        _attachmentContext.AttachmentMetadata.Add(attachment.Metadata);
        await _attachmentContext.SaveChangesAsync(ct);

        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = BucketName,
            Key = attachment.Metadata.FileKey,
            InputStream = attachment.Content,
            ContentType = attachment.Metadata.ContentType
        }, ct);

        await tx.CommitAsync(ct);
    }

    public async Task DeleteAttachment(Guid attachmentId, CancellationToken ct)
    {
        var attachment = await _attachmentContext.AttachmentMetadata.FindAsync([attachmentId], ct);
        if (attachment == null) return;

        using var tx = await _attachmentContext.Database.BeginTransactionAsync(ct);

        _attachmentContext.AttachmentMetadata.Remove(attachment);
        await _attachmentContext.SaveChangesAsync(ct);

        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = BucketName,
            Key = attachment.FileKey
        }, ct);

        await tx.CommitAsync(ct);
    }

    public async Task<Attachment?> GetAttachment(Guid attachmentId, CancellationToken ct)
    {
        var metadata = await _attachmentContext.AttachmentMetadata
            .FirstOrDefaultAsync(a => a.Id == attachmentId, ct);

        if (metadata == null)
            return null;
        
        var response = await _s3Client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = BucketName,
            Key = metadata.FileKey
        }, ct);

        var attachment = new Attachment(metadata, response.ResponseStream);

        return attachment;
    }

    public async Task<IEnumerable<AttachmentMetadata>> GetPostAttachmentMetadata(Guid postId, CancellationToken ct)
        => await _attachmentContext.AttachmentMetadata.Where(a => a.PostId == postId).ToListAsync(ct);
}
