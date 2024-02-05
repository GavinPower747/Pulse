using System.Data;
using Dapper;
using MassTransit;
using Pulse.Posts.Contracts;
using Pulse.Posts.Contracts.Messages;
using Pulse.Posts.Domain;
using Pulse.Posts.Domain.Mapping;

namespace Pulse.Posts.Services;

internal class PostCreator(IDbConnection dbConnection, IBus messageBus, DomainDtoMapper mapper)
    : IPostCreator
{
    private readonly IDbConnection _dbConnection = dbConnection;
    private readonly IBus _messageBus = messageBus;
    private readonly DomainDtoMapper _mapper = mapper;

    private const string CreatePostSql =
        @"
        INSERT INTO posts (id, user_id, content, created_at)
        VALUES (@Id, @UserId, @Content, @CreatedAt)
    ";

    public async Task<DisplayPost> Create(Guid userId, string content)
    {
        var post = new Post(Guid.NewGuid(), userId, content, DateTime.UtcNow, null, null);

        await _dbConnection.ExecuteAsync(CreatePostSql, post);
        await _messageBus.Publish(new PostCreatedEvent(post.Id, post.UserId, post.CreatedAt));

        return _mapper.MapToDisplayPost(post);
    }

    public Task<DisplayPost> Schedule(DateTime scheduleFor)
    {
        throw new NotImplementedException();
    }
}
