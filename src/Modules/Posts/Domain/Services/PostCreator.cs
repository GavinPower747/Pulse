using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Contracts;
using Pulse.Posts.Contracts.Messages;
using Pulse.Posts.Data;
using Pulse.Posts.Domain;
using Pulse.Posts.Domain.Mapping;
using Pulse.Shared.Messaging;

namespace Pulse.Posts.Services;

internal class PostCreator(
    IDbContextFactory<PostsContext> ctxFactory,
    IProducer messageBus,
    DomainDtoMapper mapper
) : IPostCreator
{
    private readonly IDbContextFactory<PostsContext> _contextFactory = ctxFactory;
    private readonly IProducer _messageBus = messageBus;
    private readonly DomainDtoMapper _mapper = mapper;

    public async Task<DisplayPost> Create(
        Guid? postId,
        Guid userId,
        string content,
        CancellationToken ct = default
    )
    {
        var post = new Post(postId ?? Guid.NewGuid(), userId, content, DateTime.UtcNow, null);

        using var dbConnection = await _contextFactory.CreateDbContextAsync(ct);
        await using var transaction = await dbConnection.Database.BeginTransactionAsync(ct);

        dbConnection.PostSet.Add(post);
        await dbConnection.SaveChangesAsync(ct);
        await _messageBus.Publish(
            new PostCreatedEvent(post.Id, post.UserId, post.CreatedAt, post.Content),
            ct
        );

        await transaction.CommitAsync(ct);

        return _mapper.MapToDisplayPost(post);
    }

    public Task<DisplayPost> Schedule(DateTime scheduleFor, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
