using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Autofac;
using Autofac.Builder;

namespace Pulse.Posts.Extensions;

public static class DIExtensions
{
    public static IRegistrationBuilder<
        TImplementer,
        ConcreteReflectionActivatorData,
        SingleRegistrationStyle
    > RegisterIsolated<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementer
    >(this ContainerBuilder builder, [CallerMemberName] string? callerMemberName = null)
        where TImplementer : notnull
    {
        return builder.RegisterType<TImplementer>().AsSelf().InstancePerLifetimeScope();
    }
}
