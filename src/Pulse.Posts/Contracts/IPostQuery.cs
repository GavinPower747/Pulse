namespace Pulse.Posts.Contracts;

public interface IPostQueryService
{
    Task<DisplayPost?> Get(Guid id, CancellationToken token);
    Task<IEnumerable<DisplayPost>> Get(IEnumerable<Guid> ids, CancellationToken token);
    Task<PostPage> GetForUser(
        Guid userId,
        int pageSize,
        string? continuationToken,
        CancellationToken token
    );
}

public record PostPage(IEnumerable<DisplayPost> Posts, string? ContinuationToken);
