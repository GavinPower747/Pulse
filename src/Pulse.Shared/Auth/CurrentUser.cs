using Microsoft.AspNetCore.Http;

namespace Pulse.Shared.Auth;

/// <summary>
/// The current logged in user, mapped from the HttpContext.
/// </summary>
public class CurrentUser(HttpContext context)
{
    public Guid Id = context.GetUserId();
    public string UserName = context.User.Identity?.Name ?? "Anonymous";
    public string DisplayName = context.User.FindFirst("name")?.Value ?? "Anonymous";
    public bool IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
    public string ProfilePictureUrl = string.Empty;
    public string Initials =>
        DisplayName.Split(' ').Select(x => x[0]).Aggregate(string.Empty, (a, b) => a + b).ToUpper();
}
