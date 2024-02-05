using FluentAssertions;
using Pulse.Posts.Services;
using Pulse.Posts.Tests.Fixtures;

namespace Pulse.Posts.Tests.Services;

public class PostCreatorTests(DatabaseFixture databaseFixture) : IClassFixture<DatabaseFixture>
{
    private readonly PostCreator _sut = new(databaseFixture.Connection);

    [Fact]
    public async Task GivenValidPost_CreatePost_Should_ReturnCreatedPost()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var content = new Bogus.DataSets.Rant().Review();

        // Act
        var result = await _sut.Create(userId, content);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.UserId.Should().Be(userId);
        result.Content.Should().Be(content);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }
}
