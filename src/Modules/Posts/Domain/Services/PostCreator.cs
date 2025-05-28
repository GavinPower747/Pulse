using Pulse.Posts.Contracts;
using Pulse.Posts.Contracts.Messages;
using Pulse.Posts.Data;
using Pulse.Posts.Domain;
using Pulse.Posts.Domain.Mapping;
using Pulse.Shared.Messaging;

namespace Pulse.Posts.Services;

internal class PostCreator(PostsContext db, IProducer messageBus, DomainDtoMapper mapper) : IPostCreator
{
    private readonly PostsContext _dbConnection = db;
    private readonly IProducer _messageBus = messageBus;
    private readonly DomainDtoMapper _mapper = mapper;

    public async Task<DisplayPost> Create(Guid userId, string content)
    {
        var post = new Post(Guid.NewGuid(), userId, content, DateTime.UtcNow, null);

        await using var transaction = await _dbConnection.Database.BeginTransactionAsync();

        _dbConnection.PostSet.Add(post);
        await _dbConnection.SaveChangesAsync();
        await _messageBus.Publish(new PostCreatedEvent(post.Id, post.UserId, post.CreatedAt));

        await transaction.CommitAsync();

        return _mapper.MapToDisplayPost(post);
    }

    public Task<DisplayPost> Schedule(DateTime scheduleFor)
    {
        throw new NotImplementedException();
    }
}
