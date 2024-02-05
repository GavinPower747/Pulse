using System.Data;
using Bogus;
using Dapper;
using FluentAssertions;
using Pulse.Posts.Domain;
using Pulse.Posts.Domain.Mapping;
using Pulse.Posts.Services;
using Pulse.Posts.Tests.Data;
using Pulse.Posts.Tests.Fixtures;

namespace Pulse.Posts.Tests.Services;

public class PostQueryServiceTests : IClassFixture<DatabaseFixture>
{
    internal readonly PostQueryService Sut;
    protected readonly DatabaseFixture _fixture;
    protected IDbConnection Connection => _fixture.Connection;

    public PostQueryServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        Sut = new PostQueryService(Connection, new DomainDtoMapper());
    }

    public class Given_Existing : PostQueryServiceTests
    {
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Post _existingPost;
        private readonly List<Post> _existingPosts = [];

        public Given_Existing(DatabaseFixture fixture)
            : base(fixture)
        {
            const string insertStatement = """
                INSERT INTO Posts 
                (
                    id, 
                    user_id, 
                    content, 
                    created_at, 
                    published_at, 
                    scheduled_at
                ) 
                VALUES (
                    @Id, 
                    @UserId, 
                    @Content, 
                    @CreatedAt,
                    @PublishedAt,
                    @ScheduledAt
                );
                """;
            var faker = new PostFaker().ForUser(_userId);
            var randomUserFaker = new PostFaker();

            _existingPost = faker.Generate();
            _existingPosts.Add(_existingPost);

            _existingPosts.AddRange(faker.Generate(4));
            var randomUserPosts = randomUserFaker.Generate(3);

            foreach (var post in _existingPosts.Concat(randomUserPosts))
            {
                Connection.Execute(insertStatement, post);
            }
        }

        [Fact]
        public async Task Gets_Existing_Post()
        {
            var post = await Sut.Get(_existingPost.Id, CancellationToken.None);

            post.Should().BeEquivalentTo(_existingPost);
        }

        [Fact]
        public async Task Gets_List_Of_Existing_Posts()
        {
            var posts = await Sut.Get(_existingPosts.Select(p => p.Id), CancellationToken.None);

            posts.Should().HaveCount(_existingPosts.Count);
            posts.Should().BeEquivalentTo(_existingPosts);
        }

        [Fact]
        public async Task Gets_List_Of_Existing_Posts_For_User()
        {
            var posts = await Sut.GetForUser(_existingPost.UserId, CancellationToken.None);

            posts.Should().HaveCount(_existingPosts.Count);
            posts.Should().BeEquivalentTo(_existingPosts);
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
            var posts = await Sut.GetForUser(Guid.NewGuid(), CancellationToken.None);

            posts.Should().BeEmpty();
        }
    }
}
