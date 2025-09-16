using Pulse.Posts.Contracts;
using Pulse.Shared.Aggregate;

namespace Pulse.Posts.Domain.DataRequests;

public class GetPostDataRequest(Guid postId) : AggregateDataRequest<DisplayPost>
{
    public Guid PostId { get; init; } = postId;
    public override string CacheKey => $"post:{PostId}";
}

public class GetPostDataRequestHandler(IPostQueryService postQueryService)
    : AggregateDataProvider<GetPostDataRequest, DisplayPost>
{
    private readonly IPostQueryService _postQueryService = postQueryService;

    public override async Task<AggregateDataResponse<DisplayPost>> GetData(
        GetPostDataRequest request,
        CancellationToken ct = default
    )
    {
        var post = await _postQueryService.Get(request.PostId, ct);
        return new AggregateDataResponse<DisplayPost>(post);
    }
}

public class GetPostsDataRequest(Guid[] postIds)
    : AggregateDataBatchRequest<DisplayPost, Guid>(postIds)
{
    public Guid[] PostIds { get; init; } = postIds;
    public override string CacheKey => $"posts:batch:{string.Join(',', PostIds)}";

    public override string GetItemCacheKey(Guid key) => $"post:{key}";
}

public class GetPostsDataResponse(DisplayPost[] data)
    : AggregateDataBatchResponse<DisplayPost, Guid>(data)
{
    public override DisplayPost? GetItemForKey(Guid key) => Data.FirstOrDefault(x => x.Id == key);
}

public class GetPostsDataRequestHandler(IPostQueryService postQueryService)
    : AggregateDataProvider<GetPostsDataRequest, DisplayPost[]>
{
    private readonly IPostQueryService _postQueryService = postQueryService;

    public override async Task<AggregateDataResponse<DisplayPost[]>> GetData(
        GetPostsDataRequest request,
        CancellationToken ct = default
    )
    {
        var posts = await _postQueryService.Get(request.PostIds, ct);
        return new GetPostsDataResponse([.. posts]);
    }
}
