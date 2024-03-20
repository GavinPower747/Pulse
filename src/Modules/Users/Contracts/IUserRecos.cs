namespace Pulse.Users.Contracts;

public interface IUserRecos
{
    Task<IEnumerable<User>> GetRecommendedFollows(Guid userId, CancellationToken cancellationToken);
}
