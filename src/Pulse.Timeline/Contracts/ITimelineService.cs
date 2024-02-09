namespace Pulse.Timeline.Contracts;

public interface ITimelineService
{
    Task<IEnumerable<Guid>> GetTimelinePage(
        Guid userId,
        string cursor,
        int count,
        CancellationToken cancellationToken = default
    );
}
