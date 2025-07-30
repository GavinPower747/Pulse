using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Followers.Contracts.Events;
using Pulse.Posts.Consumers;
using Pulse.Posts.Contracts;
using Pulse.Posts.Contracts.Messages;
using Pulse.Posts.Data;
using Pulse.Posts.Domain.Mapping;
using Pulse.Posts.Services;
using Pulse.Posts.UI.Mapping;
using Pulse.Shared.Data;
using Pulse.Shared.Extensions;
using Pulse.Users.Contracts.Messages;

namespace Pulse.Posts;

public class PostsModule : Module
{
    public PostsConfiguration Configuration { get; set; } = default!;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PostQueryService>().As<IPostQueryService>().SingleInstance();
        builder.RegisterType<PostCreator>().As<IPostCreator>().SingleInstance();
        builder.RegisterType<AttachmentService>().AsSelf().SingleInstance();
        builder.RegisterType<DomainDtoMapper>().AsSelf().SingleInstance();
        builder.RegisterDbContext<PostsContext>(Configuration.Database.ConnectionString);
        builder.RegisterDbContext<AttachmentContext>(Configuration.Database.ConnectionString);
        builder.RegisterType<PostMapper>().AsSelf();

        builder.Register((cfg) =>
        {
            var credentials = new BasicAWSCredentials(Configuration.BlobStorage.AccessKey, Configuration.BlobStorage.SecretKey);
            var config = new AmazonS3Config()
            {
                ServiceURL = Configuration.BlobStorage.Endpoint,
                ForcePathStyle = true,
                UseHttp = true,
                SignatureMethod = SigningAlgorithm.HmacSHA256
            };

            return new AmazonS3Client(credentials, config);
        }).As<IAmazonS3>();

        DataJobs.MigrateDatabase(
            Configuration.Database.ConnectionString,
            typeof(PostsModule).Assembly
        );

        builder.AddPostEndpoints();

        builder.RegisterConsumer<UserFollowedEvent, UserFollowedConsumer>();
        builder.RegisterConsumer<PostCreatedEvent, DetectMentions>();
        builder.RegisterConsumer<MentionValidatedEvent, MentionValidatedConsumer>();
    }
}

public class PostsConfiguration
{
    public DatabaseConfig Database { get; set; } = default!;
    public BlobStorageConfig BlobStorage { get; set; } = default!;
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = default!;
}

public class BlobStorageConfig
{
    public string Endpoint { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
}
