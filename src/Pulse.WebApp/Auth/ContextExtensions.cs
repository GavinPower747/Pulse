namespace Pulse.WebApp.Auth;

public static class ContextExtenstions
{
    private static readonly string UserIdClaimType =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

    public static Guid GetUserId(this HttpContext context)
    {
        var userId = context.User.Claims.FirstOrDefault(c => c.Type == UserIdClaimType)?.Value;

        return userId switch
        {
            null => Guid.Empty,
            _ => Guid.Parse(userId)
        };
    }
}
