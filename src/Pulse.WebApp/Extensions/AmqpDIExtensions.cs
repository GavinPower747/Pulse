using Pulse.Shared.Messaging;
using Pulse.WebApp.Configuration;
using Pulse.WebApp.Services;

using RabbitMQ.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class AmqpDIExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IConnection>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMQ").Get<MessagingConfig>();

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

        services.AddHostedService<AmqpLifetimeService>();

        return services;
    }
}