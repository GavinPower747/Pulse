using System.Data;
using Dapper;
using Pulse.Posts.Contracts;
using Pulse.Posts.Domain;
using Pulse.Posts.Domain.Mapping;

namespace Pulse.Posts.Services;

internal class PostQueryService(IDbConnection connection, DomainDtoMapper mapper)
    : IPostQueryService
{
    private readonly IDbConnection _connection = connection;
    private readonly DomainDtoMapper _mapper = mapper;

    private const string GetByIdQuery = "SELECT * FROM Posts WHERE id = @Id";
    private const string GetByUserQuery = "SELECT * FROM Posts WHERE user_id = @UserId";

    public async Task<DisplayPost?> Get(Guid id, CancellationToken cancellationToken)
    {
        var post = await _connection.QueryFirstOrDefaultAsync<Post>(GetByIdQuery, new { Id = id });

        if (post is null)
            return null;

        var postDto = _mapper.MapToDisplayPost(post);

        return postDto;
    }

    public async Task<IEnumerable<DisplayPost>> GetForUser(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var posts = await _connection.QueryAsync<Post>(GetByUserQuery, new { UserId = userId });
        var postDtos = posts.Select(_mapper.MapToDisplayPost);

        return postDtos;
    }
}
