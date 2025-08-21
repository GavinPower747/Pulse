namespace Pulse.Posts.Contracts;

public interface IPostCreator
{
    /// <summary>
    /// Create a new post from the given content.
    /// </summary>
    /// <param name="postId">The ID of the post, if null one will be generated. Won't be null if an ID was created for attachments</param>
    /// <param name="userId">The ID of the poster</param>
    /// <param name="content">The content of the post</param>
    /// <returns>The created post</returns>
    Task<DisplayPost> Create(
        Guid? postId,
        Guid userId,
        string content,
        CancellationToken ct = default
    );

    /// <summary>
    /// Schedule a post to be created at a later date.
    /// </summary>
    /// <param name="scheduleFor">The date and time to schedule the post for</param>
    /// <returns>The scheduled post</returns>
    Task<DisplayPost> Schedule(DateTime scheduleFor, CancellationToken ct = default);
}
