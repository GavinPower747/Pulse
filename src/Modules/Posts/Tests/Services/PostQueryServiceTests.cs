using System.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pulse.Posts.Data;
using Pulse.Posts.Domain;
using Pulse.Posts.Domain.Mapping;
using Pulse.Posts.Services;
using Pulse.Posts.Tests.Fakers;
using Pulse.Posts.Tests.Fixtures;

namespace Pulse.Posts.Tests.Services;

[Collection("Database")]
public class PostQueryServiceTests : IClassFixture<DatabaseFixture>
{
    internal readonly PostQueryService Sut;
    protected readonly DatabaseFixture _fixture;
    internal PostsContext Connection => _fixture.Posts;

    public PostQueryServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        var contextFactory = Substitute.For<IDbContextFactory<PostsContext>>();

        contextFactory.CreateDbContext().Returns(x => Connection);
        contextFactory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult(Connection));

        Sut = new PostQueryService(contextFactory, new DomainDtoMapper());
    }

    public class Given_Existing : PostQueryServiceTests
    {
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Post _existingPost;
        private readonly List<Post> _existingPosts = [];

        public Given_Existing(DatabaseFixture fixture)
            : base(fixture)
        {
            var faker = new PostFaker().ForUser(_userId);
            var randomUserFaker = new PostFaker();

            _existingPost = faker.Generate();
            _existingPosts.Add(_existingPost);

            _existingPosts.AddRange(faker.Generate(4));
            var randomUserPosts = randomUserFaker.Generate(3);

            foreach (var post in _existingPosts.Concat(randomUserPosts))
            {
                Connection.Add(post);
            }

            Connection.SaveChanges();
        }

        [Fact]
        public async Task Gets_Existing_Post()
        {
            var post = await Sut.Get(_existingPost.Id, CancellationToken.None);

            post.Should()
                .BeEquivalentTo(
                    _existingPost,
                    opt => opt.Excluding(p => p.UpdatedAt).Excluding(p => p.PublishedAt)
                );
        }

        [Fact]
        public async Task Gets_List_Of_Existing_Posts()
        {
            var posts = await Sut.Get(_existingPosts.Select(p => p.Id), CancellationToken.None);

            posts.Should().HaveCount(_existingPosts.Count);
            posts
                .Should()
                .BeEquivalentTo(
                    _existingPosts,
                    opt => opt.Excluding(p => p.UpdatedAt).Excluding(p => p.PublishedAt)
                );
        }

        [Fact]
        public async Task Gets_List_Of_Existing_Posts_For_User()
        {
            var page = await Sut.GetForUser(
                _existingPost.UserId,
                _existingPosts.Count,
                null,
                CancellationToken.None
            );

            page.Posts.Should().HaveCount(_existingPosts.Count);
            page.Posts.Should()
                .BeEquivalentTo(
                    _existingPosts,
                    opt => opt.Excluding(p => p.UpdatedAt).Excluding(p => p.PublishedAt)
                );
        }

        [Fact]
        public async Task Gets_List_Of_Size()
        {
            var page = await Sut.GetForUser(_existingPost.UserId, 2, null, CancellationToken.None);

            page.Posts.Should().HaveCount(2);
        }

        [Fact]
        public async Task Gets_List_With_ContinuationToken()
        {
            var page = await Sut.GetForUser(
                _existingPost.UserId,
                _existingPosts.Count - 1,
                null,
                CancellationToken.None
            );

            page.ContinuationToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Gets_All_When_Count_Greater()
        {
            var page = await Sut.GetForUser(
                _existingPost.UserId,
                _existingPosts.Count + 1,
                null,
                CancellationToken.None
            );

            page.Posts.Should().HaveCount(_existingPosts.Count);
            page.ContinuationToken.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task Gets_List_With_ContinuationToken_For_Next_Page()
        {
            var page = await Sut.GetForUser(_existingPost.UserId, 2, null, CancellationToken.None);

            var next = await Sut.GetForUser(
                _existingPost.UserId,
                2,
                page.ContinuationToken,
                CancellationToken.None
            );

            next.Should().NotBeSameAs(page);
            next.Posts.Should().HaveCount(2);
            next.ContinuationToken.Should().NotBeNullOrEmpty();
        }
    }

    public class Given_NonExisting(DatabaseFixture fixture) : PostQueryServiceTests(fixture)
    {
        [Fact]
        public async Task Gets_Null()
        {
            var post = await Sut.Get(Guid.NewGuid(), CancellationToken.None);

            post.Should().BeNull();
        }

        [Fact]
        public async Task Gets_Empty_List_For_Ids()
        {
            var posts = await Sut.Get(Enumerable.Repeat(Guid.NewGuid(), 5), CancellationToken.None);

            posts.Should().BeEmpty();
        }

        [Fact]
        public async Task Gets_Empty_List_For_User()
        {
            var page = await Sut.GetForUser(Guid.NewGuid(), 10, null, CancellationToken.None);

            page.Posts.Should().BeEmpty();
        }
    }
}
