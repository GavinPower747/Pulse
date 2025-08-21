using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Data;
using Pulse.Posts.Domain;

namespace Pulse.Posts.Services;

internal class AttachmentService(
    IAmazonS3 s3Client,
    IDbContextFactory<AttachmentContext> attachmentContext,
    BlobStorageConfig blobStorageConfig
)
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly IDbContextFactory<AttachmentContext> _attachmentContextFactory =
        attachmentContext;
    private readonly BlobStorageConfig _blobStorageConfig = blobStorageConfig;

    public async Task Upload(Attachment attachment, CancellationToken ct)
    {
        using var dbContext = await _attachmentContextFactory.CreateDbContextAsync(ct);
        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        dbContext.AttachmentMetadata.Add(attachment.Metadata);
        await dbContext.SaveChangesAsync(ct);

        await _s3Client.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = _blobStorageConfig.AttachmentsBucket,
                Key = attachment.Metadata.FileKey,
                InputStream = attachment.Content,
                ContentType = attachment.Metadata.ContentType,
            },
            ct
        );

        await tx.CommitAsync(ct);
    }

    public async Task DeleteAttachment(Guid attachmentId, CancellationToken ct)
    {
        using var attachmentContext = await _attachmentContextFactory.CreateDbContextAsync(ct);
        var attachment = await attachmentContext.AttachmentMetadata.FindAsync(
            [attachmentId],
            cancellationToken: ct
        );

        if (attachment == null)
            return;

        using var tx = await attachmentContext.Database.BeginTransactionAsync(ct);

        attachmentContext.AttachmentMetadata.Remove(attachment);
        await attachmentContext.SaveChangesAsync(ct);

        await _s3Client.DeleteObjectAsync(
            new DeleteObjectRequest
            {
                BucketName = _blobStorageConfig.AttachmentsBucket,
                Key = attachment.FileKey,
            },
            ct
        );

        await tx.CommitAsync(ct);
    }

    public async Task<Attachment?> GetAttachment(Guid attachmentId, CancellationToken ct)
    {
        using var attachmentContext = await _attachmentContextFactory.CreateDbContextAsync(ct);
        var metadata = await attachmentContext.AttachmentMetadata.FirstOrDefaultAsync(
            a => a.Id == attachmentId,
            ct
        );

        if (metadata == null)
            return null;

        var response = await _s3Client.GetObjectAsync(
            new GetObjectRequest
            {
                BucketName = _blobStorageConfig.AttachmentsBucket,
                Key = metadata.FileKey,
            },
            ct
        );

        var attachment = new Attachment(metadata, response.ResponseStream);

        return attachment;
    }

    public async Task<IEnumerable<AttachmentMetadata>> GetPostAttachmentMetadata(
        Guid postId,
        CancellationToken ct
    ) =>
        await _attachmentContextFactory
            .CreateDbContext()
            .AttachmentMetadata.Where(a => a.PostId == postId)
            .ToListAsync(ct);
}
