using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Pulse.Shared.Messaging;

public class AmqpChannelPool(
    IConnection connection,
    ILogger<AmqpChannelPool> logger,
    int maxChannels = 10
) : IAsyncDisposable
{
    private readonly IConnection _connection = connection;
    private readonly SemaphoreSlim _semaphore = new(maxChannels, maxChannels);
    private readonly ILogger<AmqpChannelPool> _logger = logger;
    private readonly ConcurrentBag<IChannel> _channels = [];

    private const int DefaultChannelTimeout = 5000;

    public async Task<IChannel> RentChannel(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);

        try
        {
            var gotPooledChannel = _channels.TryTake(out var channel);

            if (gotPooledChannel && channel!.IsClosed)
            {
                await channel.DisposeAsync();
                channel = null;
            }

            channel ??= await _connection.CreateChannelAsync(cancellationToken: ct);

            return channel;
        }
        catch (Exception ex)
        {
            _semaphore.Release();
            _logger.LogError(ex, "Error renting channel");

            throw;
        }
    }

    public void ReturnChannel(IChannel channel)
    {
        if (channel.IsOpen)
        {
            _channels.Add(channel);
        }
        else
        {
            channel
                .DisposeAsync()
                .AsTask()
                .ContinueWith(t =>
                {
                    if (t.Exception is not null)
                    {
                        _logger.LogError(t.Exception, "Error disposing channel");
                    }
                });
        }

        _semaphore.Release();
    }

    public async ValueTask UseChannel(Func<IChannel, ValueTask> action)
    {
        IChannel? channel = null;
        try
        {
            CancellationTokenSource cts = new(DefaultChannelTimeout);
            channel = await RentChannel(cts.Token);

            await action(channel);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Timeout waiting for channel");

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using channel");

            throw;
        }
        finally
        {
            if (channel is not null && channel.IsOpen)
            {
                ReturnChannel(channel);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        foreach (var channel in _channels)
        {
            try
            {
                await channel.CloseAsync();
                await channel.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing channel");
            }
        }

        _semaphore.Dispose();
    }
}
