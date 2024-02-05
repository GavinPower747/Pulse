using System.Security.Claims;

namespace Pulse.WebApp.Auth;

public static class ContextExtenstions
{
    private static string UserIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

    public static Guid GetUserId(this HttpContext context)
    {
        var userId = context.User.Claims.FirstOrDefault(c => c.Type == UserIdClaimType)?.Value;

        if (userId == null)
            throw new InvalidOperationException("User is not authenticated");

        return Guid.Parse(userId);
    }
}