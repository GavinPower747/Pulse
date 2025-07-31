namespace Pulse.Posts.Contracts;

public record AttachmentDownload(Guid Id, string DownloadUrl, string? ETag = null);
