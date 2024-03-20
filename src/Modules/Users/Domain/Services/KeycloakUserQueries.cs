using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Pulse.Users.Contracts;
using Pulse.Users.External;
using Pulse.Users.Mapping;

namespace Pulse.Users.Services;

internal class KeycloakUserQueries(KeycloakClientFactory clientFactory, UsersConfiguration config)
    : IUserQueries
{
    private readonly KeycloakClientFactory _clientFactory = clientFactory;
    private readonly UsersConfiguration _configuration = config;
    private readonly JsonSerializerOptions _jsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<User> GetUser(Guid id)
    {
        var client = await _clientFactory.GetKeycloakClient(nameof(KeycloakUserQueries));

        var response = await client.GetAsync(Keycloak.GetUser(id, _configuration.Keycloak.Realm));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
        {
            throw new Exception(content);
        }

        var keycloakUser =
            JsonSerializer.Deserialize<KeycloakUserRepresentation>(content, _jsonOptions)
            ?? throw new Exception("User not found");

        return KeycloakMapper.MapToUser(keycloakUser);
    }

    public async Task<User> GetUser(string username)
    {
        var client = await _clientFactory.GetKeycloakClient(nameof(KeycloakUserQueries));

        var response = await client.GetAsync(
            Keycloak.GetUser(username, _configuration.Keycloak.Realm)
        );
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
        {
            throw new Exception(content);
        }

        var keycloakUser =
            JsonSerializer
                .Deserialize<KeycloakUserRepresentation[]>(content, _jsonOptions)
                ?.FirstOrDefault() ?? throw new Exception("User not found");

        return KeycloakMapper.MapToUser(keycloakUser);
    }
}
