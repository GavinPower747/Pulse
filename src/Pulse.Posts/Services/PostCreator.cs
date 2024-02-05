using System.Data;

using Dapper;

using Pulse.Posts.Contracts;
using Pulse.Posts.Domain;

namespace Pulse.Posts.Services;

internal class PostCreator(IDbConnection dbConnection) : IPostCreator
{
    private readonly IDbConnection _dbConnection = dbConnection;

    private const string CreatePostSql = @"
        INSERT INTO posts (id, user_id, content, created_at)
        VALUES (@Id, @UserId, @Content, @CreatedAt)
    ";

    public async Task<DisplayPost> Create(Guid userId, string content)
    {
        var post = new Post(Guid.NewGuid(), userId, content, DateTime.UtcNow, null, null);

        await _dbConnection.ExecuteAsync(CreatePostSql, post);

        return new DisplayPost(post.Id, post.UserId, post.Content, post.CreatedAt, post.ScheduledAt, post.PublishedAt);
    }

    public Task<DisplayPost> Schedule(DateTime scheduleFor)
    {
        throw new NotImplementedException();
    }
}