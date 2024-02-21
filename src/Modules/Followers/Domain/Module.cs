using Autofac;
using Pulse.Followers.Data;
using Pulse.Followers.Domain.Services;
using Pulse.Shared.Data;
using Pulse.Shared.Extensions;

namespace Pulse.Followers;

public class FollowersModule : Module
{
    public FollowersConfiguration Configuration { get; set; } = default!;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FollowerProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterMediator(typeof(FollowersModule).Assembly);
        builder.RegisterDbContext<FollowingContext>(Configuration.Database.ConnectionString);

        DataJobs.MigrateDatabase(
            Configuration.Database.ConnectionString,
            typeof(FollowersModule).Assembly
        );
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
