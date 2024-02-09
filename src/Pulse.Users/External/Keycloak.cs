namespace Pulse.Users.External;

public class Keycloak
{
    public static string GetUser(Guid id, string realm) => $"/admin/realms/{realm}/users/{id}";

    public static string GetUser(string username, string realm) =>
        $"/admin/realms/{realm}/users?username={username}";

    public static string TokenEndpoint(string realm) =>
        $"/realms/{realm}/protocol/openid-connect/token";
}
