using Pulse.Shared.Messaging;
using Pulse.Timeline.Contracts.Commands;
using StackExchange.Redis;

namespace Pulse.Timeline.Consumers;

public class AddToTimelineConsumer(IDatabase redis)
    : IConsumer<AddPostToTimelineCommand>
{
    private readonly IDatabase _redis = redis;
    private readonly int _timelineCapacity = 100;

    public async Task Consume(AddPostToTimelineCommand evt, CancellationToken ct)
    {
        var timelineKey = $"timeline:{evt.UserId}";
        var postId = $"post:{evt.PostId}";

        double score = evt.Created.ToUnixTimeSeconds();

        bool added = await _redis.SortedSetAddAsync(timelineKey, postId, score);

        if (!added)
        {
            throw new InvalidOperationException("Failed to add post to timeline");
        }

        // If the timeline is full, remove the lowest ranked (oldest) post
        long size = await _redis.SortedSetLengthAsync(timelineKey);

        if (size > _timelineCapacity)
        {
            await _redis.SortedSetRemoveRangeByRankAsync(timelineKey, 0, 0);
        }
    }
}
