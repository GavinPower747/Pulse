namespace Pulse.Posts.Contracts;

public interface IPostQueryService
{
    Task<DisplayPost?> Get(Guid id, CancellationToken token);
    Task<IEnumerable<DisplayPost>> GetForUser(Guid userId, CancellationToken token);
}