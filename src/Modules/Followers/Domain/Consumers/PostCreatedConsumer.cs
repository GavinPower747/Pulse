using Microsoft.EntityFrameworkCore;
using Pulse.Followers.Data;
using Pulse.Followers.Domain;
using Pulse.Posts.Contracts.Messages;
using Pulse.Shared.Messaging;
using Pulse.Timeline.Contracts.Commands;

namespace Pulse.Followers.Consumer;

internal class PostCreatedConsumer(FollowingContext dbContext, IProducer messageBus)
    : IConsumer<PostCreatedEvent>
{
    private readonly FollowingContext _dbContext = dbContext;
    private readonly IProducer _messageBus = messageBus;

    public async Task Consume(PostCreatedEvent evt, CancellationToken token)
    {
        var followingId = evt.UserId;
        var followers = await _dbContext
            .Followings.Where(f => f.FollowingId == followingId)
            .ToListAsync(token);

        //Ensure we add the users post to their own timeline.
        followers.Add(new Following(followingId, followingId));

        foreach (var follower in followers)
        {
            var command = new AddPostToTimelineCommand(
                follower.UserId,
                evt.Id,
                evt.Created
            );

            await _messageBus.Publish(command, token);
        }
    }
}
