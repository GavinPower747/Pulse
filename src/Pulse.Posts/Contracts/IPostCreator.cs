namespace Pulse.Posts.Contracts;

public interface IPostCreator
{
    Task<DisplayPost> Create(Guid userId, string content);
    Task<DisplayPost> Schedule(DateTime scheduleFor);
}