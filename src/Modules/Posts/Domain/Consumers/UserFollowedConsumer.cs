﻿using MassTransit;
using Pulse.Followers.Contracts.Events;
using Pulse.Posts.Data;
using Pulse.Timeline.Contracts.Commands;

namespace Pulse.Posts;

/// <summary>
/// When one user follows another then take some recent posts from the user being followed and add them to the followers timeline.
/// </summary>
internal class UserFollowedConsumer(PostsContext dbContext, IBus bus) : IConsumer<UserFollowedEvent>
{
    private readonly PostsContext _dbContext = dbContext;
    private readonly IBus _bus = bus;

    public async Task Consume(ConsumeContext<UserFollowedEvent> context)
    {
        var posts = _dbContext.PostSet.Where(x => x.UserId == context.Message.FollowingId).Take(10);

        foreach(var post in posts)
        {
            await _bus.Publish(new AddPostToTimelineCommand(context.Message.FollowerId, post.Id, post.CreatedAt));
        }
    }
}
