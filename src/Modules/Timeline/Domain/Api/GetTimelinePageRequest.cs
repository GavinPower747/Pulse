using Microsoft.AspNetCore.Http;

namespace Pulse.WebApp.Features.Timeline.Api;

public class GetTimelinePageRequest
{
    public string? ContinuationToken { get; set; }
    public int PageSize
    {
        get => Math.Clamp(_pageSize, 1, MaxPageSize);
        set => _pageSize = value;
    }

    private int _pageSize;

    private const int MaxPageSize = 100;

    public static ValueTask<GetTimelinePageRequest> BindAsync(HttpContext context)
    {
        const string continuationTokenKey = "continuationToken";
        const string pageSizeKey = "pageSize";

        var request = new GetTimelinePageRequest
        {
            ContinuationToken = context.Request.Query[continuationTokenKey],
            PageSize = int.TryParse(context.Request.Query[pageSizeKey], out var pageSize)
                ? pageSize
                : 10
        };

        return new ValueTask<GetTimelinePageRequest>(request);
    }
}
