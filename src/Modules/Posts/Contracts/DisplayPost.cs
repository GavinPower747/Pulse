namespace Pulse.Posts.Contracts;

public record DisplayPost(Guid Id, Guid UserId, string Content, DateTime CreatedAt, IEnumerable<AttachmentDownload> Attachments)
{
    public Guid Id { get; init; } = Id;
    public Guid UserId { get; init; } = UserId;
    public string Content { get; init; } = Content;
    public DateTime CreatedAt { get; init; } = CreatedAt;
    public IEnumerable<AttachmentDownload> Attachments { get; init; } = Attachments;
}
