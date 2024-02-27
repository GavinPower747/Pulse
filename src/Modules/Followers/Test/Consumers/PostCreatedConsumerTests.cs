using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pulse.Followers.Consumer;
using Pulse.Followers.Data;
using Pulse.Followers.Domain;
using Pulse.Posts.Contracts.Messages;
using Pulse.Timeline.Contracts.Commands;

namespace Pulse.Followers.Test;

public class PostCreatedConsumerTests
{
    private readonly FollowingContext _dbContext;
    private readonly IBus _messageBus;
    private readonly PostCreatedConsumer _consumer;

    public PostCreatedConsumerTests()
    {
        var options = new DbContextOptionsBuilder<FollowingContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        var mediator = Substitute.For<IMediator>();
        _dbContext = new FollowingContext(options, mediator);
        _messageBus = Substitute.For<IBus>();
        _consumer = new PostCreatedConsumer(_dbContext, _messageBus);
    }

    [Fact]
    public async Task Consume_ShouldPublishAddPostToTimelineCommandForEachFollower()
    {
        var userId = Guid.NewGuid();
        var followerId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var created = DateTime.UtcNow;

        _dbContext.Followings.Add(new Following(followerId, userId));
        await _dbContext.SaveChangesAsync();

        var context = Substitute.For<ConsumeContext<PostCreatedEvent>>();
        context.Message.Returns(new PostCreatedEvent(postId, userId, created));

        await _consumer.Consume(context);

        await _messageBus
            .Received()
            .Publish(
                Arg.Is<AddPostToTimelineCommand>(c =>
                    c.UserId == followerId && c.PostId == postId && c.Created == created
                ),
                default
            );
    }

    [Fact]
    public async Task Consume_ShouldPublishAddPostToTimelineCommandForUserItself()
    {
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var created = DateTime.UtcNow;

        var context = Substitute.For<ConsumeContext<PostCreatedEvent>>();
        context.Message.Returns(new PostCreatedEvent(postId, userId, created));

        await _consumer.Consume(context);

        await _messageBus
            .Received()
            .Publish(
                Arg.Is<AddPostToTimelineCommand>(c =>
                    c.UserId == userId && c.PostId == postId && c.Created == created
                ),
                default
            );
    }
}
