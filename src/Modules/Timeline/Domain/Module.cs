using Autofac;
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

        builder
            .Register(cfg =>
            {
                var timelineCapacity = Configuration.TimelineCapacity;
                var redis = cfg.Resolve<IDatabase>();

                return new AddToTimelineConsumer(redis, timelineCapacity);
            })
            .As<AddToTimelineConsumer>()
            .AsImplementedInterfaces();

        builder
            .Register(cfg =>
            {
                var redis = ConnectionMultiplexer.Connect(Configuration.Redis.ConnectionString);

                return redis.GetDatabase();
            })
            .As<IDatabase>();
    }

    private void RegisterEndpoints(ContainerBuilder builder)
    {
        builder.RegisterType<GetTimelinePageEndpoint>().AsSelf();
        builder.RegisterType<GetTimelineUpdatesEndpoint>().AsSelf();
    }

    public class TimelineConfiguration
    {
        public int TimelineCapacity { get; set; }
        public RedisConfiguration Redis { get; set; } = default!;
    }
}

public class RedisConfiguration
{
    public string ConnectionString { get; set; } = default!;
}
