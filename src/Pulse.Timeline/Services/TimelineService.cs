using Pulse.Timeline.Contracts;
using StackExchange.Redis;

namespace Pulse.Timeline.Services;

public class TimelineService(IDatabase redis) : ITimelineService
{
    private readonly IDatabase _redis = redis;

    public async Task<IEnumerable<Guid>> GetTimelinePage(
        Guid userId,
        string? cursor,
        int count,
        CancellationToken token = default
    )
    {
        if (userId == Guid.Empty || count <= 0)
            return [];

        var key = $"timeline:{userId}";
        var start = string.IsNullOrEmpty(cursor) ? 0 : _redis.SortedSetRank(key, cursor) + 1;
        var end = start + count;

        var posts = await _redis.SortedSetRangeByRankAsync(
            key,
            start.GetValueOrDefault(),
            end.GetValueOrDefault()
        );

        List<Guid> postIds = [];
        foreach (var post in posts)
        {
            // We're storing the post id as "post:{id}" in the sorted set
            var _ = Guid.TryParse(post.ToString().Split(":")[1], out var postId);
            postIds.Add(postId);
        }

        return postIds;
    }
}
