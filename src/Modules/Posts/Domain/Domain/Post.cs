using System.Text.RegularExpressions;

namespace Pulse.Posts.Domain;

internal partial class Post
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? PublishedAt { get; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<AttachmentMetadata> Attachments => _attachments.AsReadOnly();
    private readonly List<AttachmentMetadata> _attachments = [];

    [GeneratedRegex(@"(?<=^|\s)@(\w+)", RegexOptions.Compiled)]
    private static partial Regex UserMentionRegex();

    public Post(
        Guid id,
        Guid userId,
        string content,
        DateTime createdAt,
        DateTime? publishedAt,
        IEnumerable<AttachmentMetadata>? attachments = null
    )
    {
        Id = id;
        UserId = userId;
        Content = content;
        CreatedAt = createdAt;
        PublishedAt = publishedAt;
        _attachments = attachments?.ToList() ?? [];
    }

    // For unit tests *sigh*
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Post() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public IEnumerable<string> GetMentionedUsernames()
    {
        return [.. UserMentionRegex().Matches(Content).Select(m => m.Groups[1].Value).Distinct()];
    }

    /// <summary>
    /// Replaces the @username in the content with a <a href="/u/username">@username</a> link.
    /// </summary>
    /// <param name="username"></param>
    public void AddMention(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return;

        Content = UserMentionRegex()
            .Replace(
                Content,
                match =>
                {
                    var mentioned = match.Groups[1].Value;
                    return string.Equals(mentioned, username, StringComparison.OrdinalIgnoreCase)
                        ? $"<a href=\"/u/{username}\">@{username}</a>"
                        : match.Value;
                }
            );
    }
}
