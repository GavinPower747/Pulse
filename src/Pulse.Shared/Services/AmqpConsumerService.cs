using System.Net.Mime;
using System.Text.Json;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulse.Shared.Messaging;
using Pulse.Shared.Messaging.Resiliance;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pulse.Shared.Services;

internal class AmqpConsumerService<T>(
    IConnection connection,
    IConsumer<T> consumer,
    ILogger<AmqpConsumerService<T>> logger,
    ImmediateRetryHandler immediateRetry,
    ExponentialDelayRetryHandler exponentialRetry
) : IHostedService
    where T : IntegrationEvent
{
    private readonly IConnection _connection = connection;
    private IChannel? _channel;
    private readonly IConsumer<T> _consumer = consumer;
    private readonly ILogger<AmqpConsumerService<T>> _logger = logger;
    private readonly ImmediateRetryHandler _immediateRetry = immediateRetry;
    private readonly ExponentialDelayRetryHandler _exponentialRetry = exponentialRetry;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System
                .Text
                .Json
                .Serialization
                .JsonIgnoreCondition
                .WhenWritingNull
        };

    public async Task StartAsync(CancellationToken ct)
    {
        var metadata = IntegrationEvent.GetEventMetadata<T>();
        var queue = metadata.GetQueueName(_consumer);

        if (string.IsNullOrEmpty(queue))
        {
            throw new ArgumentException("Queue name cannot be null or empty.", nameof(queue));
        }

        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue, true, false, false, null, cancellationToken: ct);
        await _channel.QueueBindAsync(
            queue,
            metadata.GetExchangeName(),
            string.Empty,
            null,
            cancellationToken: ct
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnEventRecieved;
        await _channel.BasicConsumeAsync(queue, false, consumer, cancellationToken: ct);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is null)
            return;

        await _channel.CloseAsync(cancellationToken);
        await _channel.DisposeAsync();
        _channel = null;
    }

    private async Task OnEventRecieved(object sender, BasicDeliverEventArgs args)
    {
        T integrationEvent;
        try
        {
            var formatter = new JsonEventFormatter();
            var contentType = args.BasicProperties.ContentType ?? "application/json";
            var cloudEvent =
                formatter.DecodeStructuredModeMessage(args.Body, new ContentType(contentType), null)
                ?? throw new JsonException("Failed to decode the message body.");
            var jsonElement =
                cloudEvent.Data as JsonElement?
                ?? throw new JsonException("Cloud event data is not a valid JSON element.");

            integrationEvent =
                JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), _jsonOptions)
                ?? throw new JsonException(
                    $"Failed to deserialize message of type {typeof(T).Name}."
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Malformed message recieved for type {EventName}", typeof(T).Name);

            // We literally can't do anything with this message, so just nack it without requeueing
            // Realistically you'd probably want to send this to a dead letter exchange or something of the like
            await _channel!.BasicNackAsync(args.DeliveryTag, false, false);
            return;
        }

        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await _consumer.Consume(integrationEvent, cts.Token);

            await _channel!.BasicAckAsync(args.DeliveryTag, false, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message of type {EventName}", typeof(T).Name);
            await HandleFailure(args, integrationEvent!);
        }
    }

    private async Task HandleFailure(
        BasicDeliverEventArgs args,
        IntegrationEvent evt,
        CancellationToken ct = default
    )
    {
        var handler = GetFailureHandler(args, evt);

        IncrementRetryCount();

        try
        {
            await handler.HandleFailure(args, evt, ct);
            await _channel!.BasicNackAsync(args.DeliveryTag, false, false, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Could not handle failure for event {EventName} using policy {PolicyName}",
                evt.GetType().Name,
                handler.GetType().Name
            );

            // If for whatever reason the failure handler can't handle then just nack and requeue the message
            await _channel!.BasicNackAsync(args.DeliveryTag, false, true, ct);
        }

        void IncrementRetryCount()
        { 
            args.BasicProperties.Headers![Messaging.Constants.Headers.RetryCount] =
                args.BasicProperties.Headers.TryGetValue(Messaging.Constants.Headers.RetryCount, out object? value) && value is int v
                    ? v + 1 
                    : 1;
        }
    }

    private IFailureHandler GetFailureHandler(BasicDeliverEventArgs args, IntegrationEvent evt)
    {
        var retryCount =
            args?.BasicProperties.Headers?.ContainsKey(Messaging.Constants.Headers.RetryCount) == true
                ? args.BasicProperties.Headers[Messaging.Constants.Headers.RetryCount] as int? ?? 0
                : 0;

        // This could probably be config, but where's the value?
        return retryCount switch
        {
            < 3 => _immediateRetry,
            < 5 => _exponentialRetry,
            _ => new DeadLetterHandler()
        };
    }
}
