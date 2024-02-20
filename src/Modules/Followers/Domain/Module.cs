using Autofac;
using Pulse.Followers.Domain.Services;
using Pulse.Shared.Extensions;

namespace Pulse.Followers;

public class FollowersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FollowerProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterMediator(typeof(FollowersModule).Assembly);
    }
}
