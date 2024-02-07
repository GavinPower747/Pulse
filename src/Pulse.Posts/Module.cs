using System.Data;
using Autofac;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Pulse.Posts.Contracts;
using Pulse.Posts.Data;
using Pulse.Posts.Domain;
using Pulse.Posts.Domain.Mapping;
using Pulse.Posts.Services;

namespace Pulse.Posts;

public class PostsModule : Module
{
    public PostsConfiguration Configuration { get; set; } = default!;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PostQueryService>().As<IPostQueryService>().SingleInstance();
        builder.RegisterType<PostCreator>().As<IPostCreator>().SingleInstance();
        builder.RegisterType<DomainDtoMapper>().AsSelf().SingleInstance();
        builder.RegisterType<PostsContext>().InstancePerLifetimeScope();
        builder
            .Register<DbContextOptions<PostsContext>>(c =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<PostsContext>();
                optionsBuilder.UseNpgsql(Configuration.Database.ConnectionString);
                return optionsBuilder.Options;
            })
            .InstancePerLifetimeScope();

        MigrateDatabase();
    }

    private void MigrateDatabase()
    {
        using (var serviceProvider = CreateServices())
        using (var scope = serviceProvider.CreateScope())
        {
            UpdateDatabase(scope.ServiceProvider);
        }
    }

    private ServiceProvider CreateServices()
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
                rb.AddPostgres()
                    .WithGlobalConnectionString(Configuration.Database.ConnectionString)
                    .ScanIn(typeof(PostsModule).Assembly)
                    .For.Migrations()
            )
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);
    }

    private static void UpdateDatabase(IServiceProvider serviceProvider)
    {
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}

public class PostsConfiguration
{
    public DatabaseConfig Database { get; set; } = default!;
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = default!;
}
