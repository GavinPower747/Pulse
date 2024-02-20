using MassTransit;
using Pulse.Timeline.Contracts.Commands;
using StackExchange.Redis;

namespace Pulse.Timeline.Consumers;

public class AddToTimelineConsumer(IDatabase redis, int timelineCapacity)
    : IConsumer<AddPostToTimelineCommand>
{
    private readonly IDatabase _redis = redis;
    private readonly int _timelineCapacity = timelineCapacity;

    public async Task Consume(ConsumeContext<AddPostToTimelineCommand> context)
    {
        var timelineKey = $"timeline:{context.Message.UserId}";
        var postId = $"post:{context.Message.PostId}";

        double score = context.Message.Created.ToUnixTimeSeconds();

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
