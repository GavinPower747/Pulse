namespace Pulse.Users.Contracts;

public record User(Guid Id, string Username, string Email, string DisplayName)
{
    public string GetUserInitials() =>
        DisplayName?.Split(' ').Select(x => x[0]).Aggregate(string.Empty, (x, y) => x + y)
        ?? string.Empty;
}
