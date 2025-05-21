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
                var evt = CreateInstanceWithDefaultValues(eventType) as IntegrationEvent;
                var name = eventType.GetProperty(nameof(IntegrationEvent.EventName))!.GetValue(evt);
                var version = eventType.GetProperty(nameof(IntegrationEvent.EventVersion))!.GetValue(evt);

                var exchangeName = $"{name}.{version}";
                await channel.ExchangeDeclareAsync(
                    exchangeName,
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

    private object? CreateInstanceWithDefaultValues(Type type)
    {
        // Get all constructors
        var constructors = type.GetConstructors();

        if (constructors.Length == 0)
        {
            return null;
        }

        // Try to find the constructor with parameters
        var constructor = constructors[0]; // Get the first constructor
        var parameters = constructor.GetParameters();

        // Create an array of default values for the parameters
        var paramValues = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            paramValues[i] = GetDefaultValue(parameters[i].ParameterType);
        }

        // Create the instance with the default parameter values
        return constructor.Invoke(paramValues);
    }

    private object GetDefaultValue(Type type)
    {
        if (type == typeof(Guid))
            return Guid.Empty;
        if (type == typeof(string))
            return string.Empty;
        if (type == typeof(int))
            return 0;
        if (type == typeof(bool))
            return false;
        if (type == typeof(DateTime))
            return DateTime.MinValue;
        if (type == typeof(Uri))
            return new Uri("http://localhost");

        // For reference types, return null
        if (!type.IsValueType)
            return null!;

        // For other value types, create a default instance
        return Activator.CreateInstance(type)!;
    }

}