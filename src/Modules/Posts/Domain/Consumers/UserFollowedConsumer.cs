using Pulse.Followers.Contracts.Events;
using Pulse.Posts.Data;
using Pulse.Shared.Messaging;
using Pulse.Timeline.Contracts.Commands;

namespace Pulse.Posts;

/// <summary>
/// When one user follows another then take some recent posts from the user being followed and add them to the followers timeline.
/// </summary>
internal class UserFollowedConsumer(PostsContext dbContext, IProducer bus) : IConsumer<UserFollowedEvent>
{
    private readonly PostsContext _dbContext = dbContext;
    private readonly IProducer _bus = bus;

    public async Task Consume(UserFollowedEvent evt, CancellationToken token)
    {
        var posts = _dbContext.PostSet.Where(x => x.UserId == evt.FollowingId).Take(10);

        foreach(var post in posts)
        {
            await _bus.Publish(new AddPostToTimelineCommand(evt.FollowerId, post.Id, post.CreatedAt), token);
        }
    }
}
