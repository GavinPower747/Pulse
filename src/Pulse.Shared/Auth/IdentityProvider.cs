
using Microsoft.AspNetCore.Http;

namespace Pulse.Shared.Auth;

public class IdentityProvider(IHttpContextAccessor httpContextAccessor)
{
    private readonly CurrentUser? _currentUser = new(httpContextAccessor.HttpContext);

    public CurrentUser? GetCurrentUser() => _currentUser;
}
