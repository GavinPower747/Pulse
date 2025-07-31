using System.Text.Json;
using Pulse.Users.Contracts;
using Pulse.Users.External;
using Pulse.Users.Mapping;

namespace Pulse.Users;

/// <summary>
///    A dumb implementation that just gets the first 5 users
/// </summary>
public class DumbUserRecos(KeycloakClientFactory clientFactory, UsersConfiguration config)
    : IUserRecos
{
    private readonly KeycloakClientFactory _clientFactory = clientFactory;
    private readonly UsersConfiguration _configuration = config;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<IEnumerable<User>> GetRecommendedFollows(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var client = await _clientFactory.GetKeycloakClient(nameof(DumbUserRecos));

        var response = await client.GetAsync(
            Keycloak.GetUsers(_configuration.Keycloak.Realm, 5),
            cancellationToken
        );
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
        {
            throw new Exception(content);
        }

        var keycloakUsers =
            JsonSerializer
                .Deserialize<KeycloakUserRepresentation[]>(content, _jsonOptions)
                ?.Where(u => u.Username != "admin" && u.Id != userId.ToString()) ?? [];

        return keycloakUsers?.Select(KeycloakMapper.MapToUser) ?? [];
    }
}
