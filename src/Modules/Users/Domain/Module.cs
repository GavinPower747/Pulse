using Autofac;
using Pulse.Users.Contracts;
using Pulse.Users.Services;

namespace Pulse.Users;

public class UsersModule : Module
{
    public required UsersConfiguration Configuration { get; set; }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(Configuration);
        builder.RegisterType<KeycloakUserQueries>().As<IUserQueries>();
    }
}

public class UsersConfiguration
{
    public required KeycloakConfiguration Keycloak { get; set; }
}

public class KeycloakConfiguration
{
    public required string ApiBase { get; set; }
    public required string Realm { get; set; }
    public required string ClientId { get; set; }
    public required string AuthUser { get; set; }
    public required string AuthPassword { get; set; }
}
