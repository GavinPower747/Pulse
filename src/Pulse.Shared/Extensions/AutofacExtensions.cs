using System.Reflection;
using Autofac;
using MediatR;
using MediatR.Pipeline;

namespace Pulse.Shared.Extensions;

public static class AutofacExtensions
{
    public static void RegisterMediator(this ContainerBuilder builder, Assembly assembly)
    {
        builder
            .RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<,>))
            .AsImplementedInterfaces();

        builder
            .RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .AsImplementedInterfaces();

        builder
            .RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(IRequestPreProcessor<>))
            .AsImplementedInterfaces();

        builder
            .RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(IRequestPostProcessor<,>))
            .AsImplementedInterfaces();
    }
}
