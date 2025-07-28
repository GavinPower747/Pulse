namespace Pulse.Users.Contracts;

public interface IUserQueries
{
    Task<User> GetUser(Guid id);
    Task<User> GetUser(string username);
    Task<bool> UserExists(string username);
}

