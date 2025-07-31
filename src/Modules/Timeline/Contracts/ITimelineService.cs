namespace Pulse.Timeline.Contracts;

public interface ITimelineService
{
    Task<TimelinePage> GetTimelinePage(
        Guid userId,
        string cursor,
        int count,
        CancellationToken cancellationToken = default
    );

    Task<(bool hasChanges, string newEtag)> CheckForChanges(
        Guid userId,
        string etag,
        CancellationToken cancellationToken = default
    );
}

public record TimelinePage(IEnumerable<Guid> Ids, string Etag, string ContinuationToken);
