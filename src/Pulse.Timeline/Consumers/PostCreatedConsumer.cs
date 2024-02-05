using MassTransit;
using Microsoft.Extensions.Logging;
using Pulse.Posts.Contracts.Messages;
using StackExchange.Redis;

namespace Pulse.Timeline.Consumers;

public class PostCreatedConsumer(IDatabase redis) : IConsumer<PostCreatedEvent>
{
    private readonly IDatabase _redis = redis;

    public async Task Consume(ConsumeContext<PostCreatedEvent> context)
    {
        var timelineKey = $"timeline:{context.Message.UserId}"; // For now, we'll just add the post to the user's own timeline. Later we can get everyone that's following them and fan out
        var postId = $"post:{context.Message.Id}";

        double score = context.Message.Created.ToUnixTimeSeconds();

        bool added = await _redis.SortedSetAddAsync(timelineKey, postId, score);

        if (!added)
        {
            throw new InvalidOperationException("Failed to add post to timeline");
        }
    }
}
