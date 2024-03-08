using System.Text;
using Pulse.Timeline.Contracts;
using StackExchange.Redis;

namespace Pulse.Timeline.Services;

public class TimelineService(IDatabase redis) : ITimelineService
{
    private readonly IDatabase _redis = redis;

    public async Task<TimelinePage> GetTimelinePage(
        Guid userId,
        string? cursor,
        int count,
        CancellationToken token = default
    )
    {
        if (userId == Guid.Empty || count <= 0)
            return new TimelinePage([], string.Empty, string.Empty);

        var key = $"timeline:{userId}";
        var start = string.IsNullOrEmpty(cursor) ? 0 : _redis.SortedSetRank(key, cursor) + 1;
        var end = start + count;

        var posts = await _redis.SortedSetRangeByRankAsync(
            key,
            start.GetValueOrDefault(),
            end.GetValueOrDefault(),
            Order.Descending
        );

        List<Guid> postIds = [];
        foreach (var post in posts)
        {
            // We're storing the post id as "post:{id}" in the sorted set
            var _ = Guid.TryParse(post.ToString().Split(":")[1], out var postId);
            postIds.Add(postId);
        }

        string etag = posts.Length > 0 ? new ChangesEtag(posts[0].ToString()) : string.Empty;

        return new TimelinePage(
            postIds,
            etag,
            posts.Length == count ? posts[^1].ToString() : string.Empty
        );
    }

    public async Task<(bool hasChanges, string newEtag)> CheckForChanges(
        Guid userId,
        string etag,
        CancellationToken token = default
    )
    {
        var parsedTag = ChangesEtag.Parse(etag);
        var key = $"timeline:{userId}";
        var topPost = await _redis.SortedSetRangeByRankAsync(key, 0, 0, Order.Descending);

        if (topPost.Length == 0)
            return (false, string.Empty);

        var lastUpdate = topPost[0].ToString();

        return (lastUpdate != parsedTag.CacheKey, new ChangesEtag(lastUpdate).ToString());
    }

    /// <summary>
    /// A simple wrapper around etag encoding to make it more readable
    /// </summary>
    /// <param name="cacheKey">The most recent cache key in the timeline at the time of reading</param>
    private readonly struct ChangesEtag(string cacheKey)
    {
        public string CacheKey { get; } = cacheKey;

        public static ChangesEtag Parse(string etag)
        {
            var unencoded = Encoding.UTF8.GetString(Convert.FromBase64String(etag));

            return new ChangesEtag(unencoded);
        }

        public static implicit operator string(ChangesEtag etag) => etag.ToString();

        public override readonly string ToString() =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(CacheKey));
    }
}
