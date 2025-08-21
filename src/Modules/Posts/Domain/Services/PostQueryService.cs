using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Contracts;
using Pulse.Posts.Data;
using Pulse.Posts.Domain.Mapping;
using Pulse.Shared.Encoders;

namespace Pulse.Posts.Services;

internal class PostQueryService(IDbContextFactory<PostsContext> connection, DomainDtoMapper mapper)
    : IPostQueryService
{
    private readonly IDbContextFactory<PostsContext> _contextFactory = connection;
    private readonly DomainDtoMapper _mapper = mapper;

    public async Task<DisplayPost?> Get(Guid id, CancellationToken ct)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync(ct);
        var post = await dbContext.PostSet.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (post is null)
            return null;

        var postDto = _mapper.MapToDisplayPost(post);

        return postDto;
    }

    public async Task<IEnumerable<DisplayPost>> Get(IEnumerable<Guid> ids, CancellationToken ct)
    {
        if (!ids.Any())
            return [];

        using var dbContext = await _contextFactory.CreateDbContextAsync(ct);
        var posts = await dbContext
            .PostSet.Include(p => p.Attachments)
            .Where(p => ids.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        var postDtos = posts.Select(_mapper.MapToDisplayPost);

        return postDtos;
    }

    public async Task<PostPage> GetForUser(
        Guid userId,
        int pageSize,
        string? continuationToken,
        CancellationToken ct
    )
    {
        var token = ContinuationToken.Parse(continuationToken);
        using var dbContext = await _contextFactory.CreateDbContextAsync(ct);
        var postsQuery = dbContext.PostSet.Where(p => p.UserId == userId);

        if (token is not null)
            postsQuery = postsQuery.Where(p => p.CreatedAt >= token.Value.OlderThan);

        postsQuery = postsQuery
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(pageSize + 1); // Take one extra to check if there are more pages

        var posts = await postsQuery.ToListAsync(ct);

        var postDtos = posts.Take(pageSize).Select(_mapper.MapToDisplayPost);

        string? newToken = null;
        if (posts.Count > pageSize)
        {
            var overflowPost = posts.Last();
            newToken = new ContinuationToken(overflowPost.CreatedAt, overflowPost.Id).ToString();
        }

        return new PostPage(postDtos, newToken);
    }

    private readonly struct ContinuationToken(DateTime olderThan, Guid lastRecord)
    {
        // Continue from a specific datetime rather than skip/take in case a new post gets added between changes
        public DateTime OlderThan { get; } = olderThan;
        public Guid LastRecord { get; } = lastRecord;

        public static ContinuationToken? Parse(string? token)
        {
            if (token is null)
                return null;

            var unencoded = Base64UrlEncoder.Decode(token);
            var decoded = Encoding.UTF8.GetString(unencoded);
            var split = decoded.Split('|');

            return new ContinuationToken(DateTime.Parse(split[0]), Guid.Parse(split[1]));
        }

        public override readonly string ToString()
        {
            var separated = $"{OlderThan}|{LastRecord}";
            var encoded = Encoding.UTF8.GetBytes(separated);
            var token = Base64UrlEncoder.Encode(encoded);

            return token;
        }
    }
}
