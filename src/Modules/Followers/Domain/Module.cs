using Autofac;

using Microsoft.Extensions.DependencyInjection;

using Pulse.Followers.Api.Endpoints;
using Pulse.Followers.Consumer;
using Pulse.Followers.Contracts.Events;
using Pulse.Followers.Data;
using Pulse.Followers.Domain.Services;
using Pulse.Posts.Contracts.Messages;
using Pulse.Shared.Data;
using Pulse.Shared.Extensions;

namespace Pulse.Followers;

public class FollowersModule : Module
{
    public FollowersConfiguration Configuration { get; set; } = default!;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FollowerProvider>().AsImplementedInterfaces();
        builder.RegisterMediator(typeof(FollowersModule).Assembly);
        builder.RegisterDbContext<FollowingContext>(Configuration.Database.ConnectionString);

        DataJobs.MigrateDatabase(
            Configuration.Database.ConnectionString,
            typeof(FollowersModule).Assembly
        );

        RegisterEndpoints(builder);
        RegisterMessageConsumers(builder);
    }

    private static void RegisterEndpoints(ContainerBuilder builder)
    {
        builder.RegisterType<AddFollowerEndpoint>().AsSelf();
        builder.RegisterType<RemoveFollowerEndpoint>().AsSelf();
    }

    private static void RegisterMessageConsumers(ContainerBuilder builder)
    {
        builder.RegisterConsumer<PostCreatedEvent, PostCreatedConsumer>();
    }
}

public class FollowersConfiguration
{
    public DatabaseConfig Database { get; set; } = default!;
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = default!;
}
