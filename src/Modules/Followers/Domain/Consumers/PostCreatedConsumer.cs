using MassTransit;
using Pulse.Followers.Data;
using Pulse.Posts.Contracts.Messages;
using Pulse.Timeline.Contracts.Commands;

namespace Pulse.Followers.Consumer;

internal class PostCreatedConsumer(FollowingContext dbContext, IBus messageBus)
    : IConsumer<PostCreatedEvent>
{
    private readonly FollowingContext _dbContext = dbContext;
    private readonly IBus _messageBus = messageBus;

    public async Task Consume(ConsumeContext<PostCreatedEvent> context)
    {
        var followingId = context.Message.UserId;
        var followers = _dbContext.Followings.Where(f => f.FollowingId == followingId);

        foreach (var follower in followers)
        {
            var command = new AddPostToTimelineCommand(
                follower.UserId,
                context.Message.Id,
                context.Message.Created
            );

            await _messageBus.Send(command);
        }
    }
}
