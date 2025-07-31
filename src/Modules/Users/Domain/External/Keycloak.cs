namespace Pulse.Users.External;

public class Keycloak
{
    public static string GetUser(Guid id, string realm) => $"/admin/realms/{realm}/users/{id}";

    public static string GetUser(string username, string realm) =>
        $"/admin/realms/{realm}/users?username={username}";

    public static string GetUsers(string realm, int max = 5) =>
        $"/admin/realms/{realm}/users?max={max}&first=0&exclude=admin";

    public static string TokenEndpoint(string realm) =>
        $"/realms/{realm}/protocol/openid-connect/token";
}
