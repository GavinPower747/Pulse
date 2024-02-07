using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Contracts;
using Pulse.Posts.Data;
using Pulse.Posts.Domain;
using Pulse.Posts.Domain.Mapping;

namespace Pulse.Posts.Services;

internal class PostQueryService(PostsContext connection, DomainDtoMapper mapper) : IPostQueryService
{
    private readonly PostsContext _connection = connection;
    private readonly DomainDtoMapper _mapper = mapper;

    public async Task<DisplayPost?> Get(Guid id, CancellationToken cancellationToken)
    {
        var post = await _connection.PostSet.FirstOrDefaultAsync(
            p => p.Id == id,
            cancellationToken
        );

        if (post is null)
            return null;

        var postDto = _mapper.MapToDisplayPost(post);

        return postDto;
    }

    public async Task<IEnumerable<DisplayPost>> Get(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        if (!ids.Any())
            return [];

        var posts = await _connection
            .PostSet.Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var postDtos = posts.Select(_mapper.MapToDisplayPost);

        return postDtos;
    }

    public async Task<IEnumerable<DisplayPost>> GetForUser(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var posts = await _connection
            .PostSet.Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        var postDtos = posts.Select(_mapper.MapToDisplayPost);

        return postDtos;
    }
}
