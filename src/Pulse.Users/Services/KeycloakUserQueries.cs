using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Pulse.Users.Contracts;
using Pulse.Users.External;
using Pulse.Users.Mapping;

namespace Pulse.Users.Services;

internal class KeycloakUserQueries(
    IHttpClientFactory clientFactory,
    UsersConfiguration configuration
) : IUserQueries
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly UsersConfiguration _configuration = configuration;

    public async Task<User> GetUser(Guid id)
    {
        var client = await GetKeycloakClient();

        var response = await client.GetAsync(Keycloak.GetUser(id, _configuration.Keycloak.Realm));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
        {
            throw new Exception(content);
        }

        var keycloakUser =
            JsonSerializer.Deserialize<KeycloakUserRepresentation>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? throw new Exception("User not found");

        return KeycloakMapper.MapToUser(keycloakUser);
    }

    public async Task<User> GetUser(string username)
    {
        var client = await GetKeycloakClient();

        var response = await client.GetAsync(
            Keycloak.GetUser(username, _configuration.Keycloak.Realm)
        );
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
        {
            throw new Exception(content);
        }

        var keycloakUser =
            JsonSerializer.Deserialize<KeycloakUserRepresentation>(content)
            ?? throw new Exception("User not found");

        return KeycloakMapper.MapToUser(keycloakUser);
    }

    private async Task<HttpClient> GetKeycloakClient()
    {
        var client = _clientFactory.CreateClient(nameof(KeycloakUserQueries));

        client.BaseAddress = new Uri(_configuration.Keycloak.ApiBase);

        var accessToken = await GetAccessToken(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );

        return client;
    }

    private async Task<string> GetAccessToken(HttpClient client)
    {
        var uri = new Uri(
            client.BaseAddress!,
            Keycloak.TokenEndpoint(_configuration.Keycloak.Realm)
        );

        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = _configuration.Keycloak.ClientId,
                    ["username"] = _configuration.Keycloak.AuthUser,
                    ["password"] = _configuration.Keycloak.AuthPassword,
                    ["grant_type"] = "password",
                }
            )
        };

        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(content);
        }

        var token = JsonNode.Parse(content)!.AsObject();

        token.TryGetPropertyValue("access_token", out var accessToken);

        return accessToken?.ToString() ?? throw new Exception("Access token not found");
    }
}
