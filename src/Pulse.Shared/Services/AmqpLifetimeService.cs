using System.Reflection;
using Autofac;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulse.Shared.Messaging;
using RabbitMQ.Client;

namespace Pulse.WebApp.Services;

public class AmqpService(IConnection connection, AmqpChannelPool channelPool, ILogger<AmqpService> logger, Assembly[] searchAssemblies) : IHostedService
{
    private readonly IConnection _connection = connection;
    private readonly AmqpChannelPool _channelPool = channelPool;
    private readonly ILogger<AmqpService> _logger = logger;
    private readonly Assembly[] _searchAssemblies = searchAssemblies;

    public async Task StartAsync(CancellationToken ct)
    {
        await SetupExchanges(ct);
    }

    public async Task StopAsync(CancellationToken ct)
    {
        await _channelPool.DisposeAsync();
        await _connection.CloseAsync(ct);
        await _connection.DisposeAsync();
    }

    private async Task SetupExchanges(CancellationToken ct)
    {
        var integrationEvents = _searchAssemblies
            .SelectMany(x => x!.GetTypes())
            .Where(x =>
                x.IsClass
                && !x.IsAbstract
                && x.IsAssignableTo<IntegrationEvent>()
            );

        IChannel? channel = null;
        try
        {
            channel = await _channelPool.RentChannel(ct);
            foreach (var eventType in integrationEvents)
            {
                var metadata = IntegrationEvent.GetEventMetadata(eventType);

                await channel.ExchangeDeclareAsync(
                    metadata.GetExchangeName(),
                    ExchangeType.Fanout,
                    cancellationToken: ct
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
}
