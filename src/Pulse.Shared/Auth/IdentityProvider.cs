using Microsoft.AspNetCore.Http;

namespace Pulse.Shared.Auth;

public class IdentityProvider(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public CurrentUser GetCurrentUser() => new(_httpContextAccessor.HttpContext);
}
