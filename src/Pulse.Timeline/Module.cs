using Autofac;
using MassTransit;
using Pulse.Timeline.Consumers;
using Pulse.Timeline.Contracts;
using Pulse.Timeline.Services;
using StackExchange.Redis;

namespace Pulse.Timeline;

public class TimelineModule : Module
{
    public TimelineConfiguration Configuration { get; set; } = default!;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TimelineService>().As<ITimelineService>().SingleInstance();
        builder.RegisterType<PostCreatedConsumer>().AsSelf().AsImplementedInterfaces();
        builder.Register(cfg =>
        {
            var redis = ConnectionMultiplexer.Connect(Configuration.Redis.ConnectionString);

            return redis.GetDatabase();
        });
    }

    public class TimelineConfiguration
    {
        public RedisConfiguration Redis { get; set; } = default!;
    }
}

public class RedisConfiguration
{
    public string ConnectionString { get; set; } = default!;
}
