using Microsoft.AspNetCore.Http;

namespace Pulse.Shared.Auth;

/// <summary>
/// The current logged in user, mapped from the HttpContext.
/// </summary>
public class CurrentUser(HttpContext context)
{
    private readonly HttpContext _context = context;

    public Guid Id => _context.GetUserId();
    public string UserName => _context.User.Identity?.Name ?? "Anonymous";
    public string DisplayName => _context.User.FindFirst("name")?.Value ?? "Anonymous";
    public bool IsAuthenticated => _context.User.Identity?.IsAuthenticated ?? false;
    public string ProfilePictureUrl => string.Empty;
    public string Initials =>
        DisplayName.Split(' ').Select(x => x[0]).Aggregate(string.Empty, (a, b) => a + b).ToUpper();
}
