using System.Net.Mime;
using System.Text.Json;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulse.Shared.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pulse.WebApp.Services;

public class AmqpConsumerService<T>(IConnection connection, IConsumer<T> consumer, ILogger<AmqpConsumerService<T>> logger) : IHostedService where T : IntegrationEvent
{
    private readonly IConnection _connection = connection;
    private IChannel? _channel;
    private readonly IConsumer<T> _consumer = consumer;
    private readonly ILogger<AmqpConsumerService<T>> _logger = logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
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
        await _channel.QueueBindAsync(queue, metadata.GetExchangeName(), string.Empty, null, cancellationToken: ct);

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
        try
        {
            var formatter = new JsonEventFormatter();
            var contentType = args.BasicProperties.ContentType ?? "application/json";
            var cloudEvent = formatter.DecodeStructuredModeMessage(args.Body, new ContentType(contentType), null) ?? throw new JsonException("Failed to decode the message body.");
            var jsonElement = cloudEvent.Data as JsonElement? ?? throw new JsonException("Cloud event data is not a valid JSON element.");
            var integrationEvent = JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), _jsonOptions) ?? throw new JsonException($"Failed to deserialize message of type {typeof(T).Name}.");

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await _consumer.Consume(integrationEvent, cts.Token);

            await _channel!.BasicAckAsync(args.DeliveryTag, false, cts.Token);
        }
        catch (Exception ex)
        {
            await _channel!.BasicNackAsync(args.DeliveryTag, false, false);
            _logger.LogError(ex, "Error processing message of type {EventName}", typeof(T).Name);

            throw;
        }
    }
}
