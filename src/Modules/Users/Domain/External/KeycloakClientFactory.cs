using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Pulse.Users.External;

namespace Pulse.Users;

public class KeycloakClientFactory(
    IHttpClientFactory clientFactory,
    UsersConfiguration configuration
)
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly UsersConfiguration _configuration = configuration;

    public async Task<HttpClient> GetKeycloakClient(string useCase)
    {
        var client = _clientFactory.CreateClient(useCase);

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
            ),
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
