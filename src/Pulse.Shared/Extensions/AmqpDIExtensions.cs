using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulse.Shared.Messaging;
using Pulse.Shared.Messaging.Resiliance;
using Pulse.Shared.Services;
using Pulse.WebApp.Configuration;
using Pulse.WebApp.Services;
using RabbitMQ.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class AmqpDIExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        Assembly[] searchAssemblies
    )
    {
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>()
                .GetSection("RabbitMQ")
                .Get<MessagingConfig>();

            ArgumentException.ThrowIfNullOrEmpty(config?.Host, nameof(config.Host));
            ArgumentException.ThrowIfNullOrEmpty(config?.UserName, nameof(config.UserName));
            ArgumentException.ThrowIfNullOrEmpty(config?.Password, nameof(config.Password));

            ConnectionFactory factory = new()
            {
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(30),
                HostName = config.Host,
                UserName = config.UserName,
                Password = config.Password,
            };

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddSingleton<AmqpChannelPool>();
        services.AddSingleton<IProducer, AmqpPublisher>();
        services.AddSingleton<ImmediateRetryHandler>();
        services.AddSingleton<ExponentialDelayRetryHandler>();
        services.AddSingleton<DeadLetterHandler>();

        services.AddSingleton<IHostedService, AmqpService>(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            var channelPool = sp.GetRequiredService<AmqpChannelPool>();
            var logger = sp.GetRequiredService<ILogger<AmqpService>>();
            var consumers = sp.GetServices<IConsumer>().ToList();

            return new AmqpService(connection, channelPool, logger, searchAssemblies);
        });

        return services;
    }

    public static ContainerBuilder RegisterConsumer<TMsg, THandler>(this ContainerBuilder services)
        where TMsg : IntegrationEvent
        where THandler : class, IConsumer<TMsg>
    {
        services.RegisterType<THandler>().As<IConsumer>().SingleInstance();
        services.RegisterType<THandler>().As<IConsumer<TMsg>>().SingleInstance();
        services.RegisterType<THandler>().AsSelf().SingleInstance();
        services
            .Register(sp =>
            {
                var connection = sp.Resolve<IConnection>();
                var consumer = sp.Resolve(typeof(THandler)) as IConsumer<TMsg>;
                var logger = sp.Resolve<ILogger<AmqpConsumerService<TMsg>>>();
                var immediateRetry = sp.Resolve<ImmediateRetryHandler>();
                var exponentialRetry = sp.Resolve<ExponentialDelayRetryHandler>();
                var deadLetterHandler = sp.Resolve<DeadLetterHandler>();

                return new AmqpConsumerService<TMsg>(
                    connection,
                    consumer!,
                    logger,
                    immediateRetry,
                    exponentialRetry,
                    deadLetterHandler
                );
            })
            .As<IHostedService>()
            .SingleInstance();

        return services;
    }
}
