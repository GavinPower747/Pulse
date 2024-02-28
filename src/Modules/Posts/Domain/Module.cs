using Autofac;
using Pulse.Posts.Contracts;
using Pulse.Posts.Data;
using Pulse.Posts.Domain.Mapping;
using Pulse.Posts.Services;
using Pulse.Posts.UI.Mapping;
using Pulse.Shared.Data;
using Pulse.Shared.Extensions;
using Pulse.WebApp.Features.Posts.API;

namespace Pulse.Posts;

public class PostsModule : Module
{
    public PostsConfiguration Configuration { get; set; } = default!;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PostQueryService>().As<IPostQueryService>().SingleInstance();
        builder.RegisterType<PostCreator>().As<IPostCreator>().SingleInstance();
        builder.RegisterType<DomainDtoMapper>().AsSelf().SingleInstance();
        builder.RegisterDbContext<PostsContext>(Configuration.Database.ConnectionString);
        builder.RegisterType<PostMapper>().AsSelf();

        DataJobs.MigrateDatabase(
            Configuration.Database.ConnectionString,
            typeof(PostsModule).Assembly
        );

        builder.AddPostEndpoints();
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
