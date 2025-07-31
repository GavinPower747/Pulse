using Pulse.Posts.Consumers;
using Pulse.Posts.Contracts.Messages;
using Pulse.Tests.Util.Fixtures;

namespace Pulse.Posts.Tests.Consumers;

public class DetectMentionsTests : IClassFixture<RabbitMqFixture>
{
    private readonly DetectMentions _sut;
    private readonly RabbitMqFixture _rabbitFixture;
    private const string ConsumerName = nameof(DetectMentions);

    public DetectMentionsTests(RabbitMqFixture rabbitFixture)
    {
        _rabbitFixture = rabbitFixture;
        _sut = new DetectMentions(_rabbitFixture.GetProducer());

        _rabbitFixture.DeclareForEvent<UserMentionedEvent>(ConsumerName).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Consume_Should_Publish_UserMentionedEvent_When_MentionsExist()
    {
        // Arrange
        var postCreatedEvent = new PostCreatedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Hello @user1 and @user2!"
        );

        // Act
        await _sut.Consume(postCreatedEvent, CancellationToken.None);

        // Assert
        var messages = _rabbitFixture.GetMessagesForEvent<UserMentionedEvent>(ConsumerName);
        Assert.Equal(2, messages.Count());
    }

    [Fact]
    public async Task Consume_Should_NotPublish_When_NoMentionsExist()
    {
        // Arrange
        var postCreatedEvent = new PostCreatedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Hello, world!"
        );

        // Act
        await _sut.Consume(postCreatedEvent, CancellationToken.None);

        // Assert
        var messages = _rabbitFixture.GetMessagesForEvent<UserMentionedEvent>(ConsumerName);
        Assert.Empty(messages);
    }
}
