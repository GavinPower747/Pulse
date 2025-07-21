using FluentAssertions;
using NSubstitute;
using Pulse.Posts.Contracts.Messages;
using Pulse.Posts.Domain.Mapping;
using Pulse.Posts.Services;
using Pulse.Posts.Tests.Fixtures;
using Pulse.Shared.Messaging;

namespace Pulse.Posts.Tests.Services;

[Collection("Database")]
public class PostCreatorTests : IClassFixture<DatabaseFixture>
{
    private readonly IProducer _messageBus = Substitute.For<IProducer>();
    private readonly PostCreator _sut;

    public PostCreatorTests(DatabaseFixture databaseFixture)
    {
        _sut = new PostCreator(databaseFixture.Posts, _messageBus, new DomainDtoMapper());
    }

    [Fact]
    public async Task GivenValidPost_CreatePost_Should_ReturnCreatedPost()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var content = new Bogus.DataSets.Rant().Review();

        // Act
        var result = await _sut.Create(null, userId, content);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.UserId.Should().Be(userId);
        result.Content.Should().Be(content);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task GivenValidPost_CreatePost_Should_PublishPostCreatedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var content = new Bogus.DataSets.Rant().Review();

        // Act
        await _sut.Create(null, userId, content);

        // Assert
        await _messageBus.Received(1).Publish(Arg.Any<PostCreatedEvent>());
    }
}
