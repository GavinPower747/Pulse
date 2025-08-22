using System.Reflection;
using Autofac;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Identity.Client;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

    public static void RegisterDbContext<TContext>(
        this ContainerBuilder builder,
        string connectionString
    )
        where TContext : DbContext
    {
        builder
            .Register(c =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<TContext>();
                optionsBuilder.UseNpgsql(connectionString);
                return optionsBuilder.Options;
            })
            .As<DbContextOptions<TContext>>()
            .SingleInstance();

        builder
            .RegisterType<PooledDbContextFactory<TContext>>()
            .As<IDbContextFactory<TContext>>()
            .SingleInstance();

        builder.RegisterType<TContext>().AsSelf().InstancePerLifetimeScope();
    }
}
