using Pulse.Shared.Messaging;
using Pulse.WebApp.Configuration;

using RabbitMQ.Client;

namespace Pulse.WebApp.Services;

public class AmqpLifetimeService(IConnection connection, AmqpChannelPool channelPool, ILogger<AmqpLifetimeService> logger, IConfiguration configuration) : IHostedService
{
    private readonly IConnection _connection = connection;
    private readonly AmqpChannelPool _channelPool = channelPool;
    private readonly ILogger<AmqpLifetimeService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var exTask = SetupExchanges(cancellationToken);
        var queueTask = SetupConsumerQueues(cancellationToken);

        await Task.WhenAll(exTask, queueTask);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _channelPool.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private async Task SetupExchanges(CancellationToken cancellationToken)
    {
        var autofacModuleAssemblies = _configuration.GetSection("modules")
            .Get<List<ModuleInfo>>();

        var moduleAssemblies = autofacModuleAssemblies!
            .Select(m => Type.GetType(m.Type)?.Assembly)
            .ToList();

        var integrationEvents = moduleAssemblies
            .SelectMany(x => x!.GetTypes())
            .Where(x =>
                x.IsClass
                && !x.IsAbstract
                && x.IsInstanceOfType(typeof(IntegrationEvent))
            );

        IChannel? channel = null;
        try
        {
            channel = await _channelPool.RentChannel(cancellationToken);
            foreach (var eventType in integrationEvents)
            {
                var evt = Activator.CreateInstance(eventType);
                var name = eventType.GetProperty(nameof(IntegrationEvent.EventName))!.GetValue(evt);
                var version = eventType.GetProperty(nameof(IntegrationEvent.EventVersion))!.GetValue(evt);

                var exchangeName = $"{name}.{version}";
                await channel.ExchangeDeclareAsync(
                    exchangeName,
                    ExchangeType.Fanout,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exchange");
            throw;
        }
        finally
        {
            if (channel is not null)
            {
                _channelPool.ReturnChannel(channel);
            }
        }
    }

    private Task SetupConsumerQueues(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}