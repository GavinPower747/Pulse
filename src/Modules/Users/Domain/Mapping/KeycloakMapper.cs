using Mapster;
using Pulse.Users.Contracts;
using Pulse.Users.External;

namespace Pulse.Users.Mapping;

internal class MapperConfig
{
    public MapperConfig()
    {
        TypeAdapterConfig<KeycloakUserRepresentation, User>
            .NewConfig()
            .Map(dest => dest.DisplayName, src => src.FirstName + " " + src.LastName);
    }
}

internal class KeycloakMapper
{
    static KeycloakMapper() => new MapperConfig();

    public static User MapToUser(KeycloakUserRepresentation keycloakUser) =>
        keycloakUser.Adapt<User>();
}
