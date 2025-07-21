namespace Pulse.Posts.Domain;

internal class Attachment(AttachmentMetadata metadata, Stream content) : IDisposable
{
    public AttachmentMetadata Metadata { get; } = metadata;
    public Stream Content { get; } = content;

    public void Dispose()
    {
        Content.Dispose();
    }

}

internal class AttachmentMetadata(Guid id, Guid postId, AttachmentType type, long size, string contentType, string eTag)
{
    public Guid Id { get; private set; } = id;
    public Guid PostId { get; private set; } = postId;
    public AttachmentType Type { get; private set; } = type;
    public long Size { get; private set; } = size;
    public string ContentType { get; private set; } = contentType;
    public string FileKey => $"{PostId}/attachments/{Id}";
    public string ETag { get; private set; } = eTag;

    public string GetFileName()
    {
        var fileExtension = ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            _ => ""
        };

        return $"{Id}{fileExtension}";
    }
}

internal enum AttachmentType
{
    Image
}
