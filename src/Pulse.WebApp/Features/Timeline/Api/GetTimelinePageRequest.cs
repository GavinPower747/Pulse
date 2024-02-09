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
}
