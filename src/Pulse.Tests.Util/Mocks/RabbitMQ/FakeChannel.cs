using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Pulse.Tests.Util.Mocks.RabbitMQ.Models;
using System.Collections.Concurrent;
using Queue = Pulse.Tests.Util.Mocks.RabbitMQ.Models.Queue;

namespace Pulse.Tests.Util.Mocks.RabbitMQ;

public class FakeChannel(RabbitServer server) : IChannel
{
    private readonly RabbitServer _server = server;
    private readonly ConcurrentDictionary<string, IAsyncBasicConsumer> _consumers = new();
    public readonly ConcurrentDictionary<ulong, RabbitMessage> WorkingMessages = new();

    private long _lastDeliveryTag;
    private bool _publisherConfirmsEnabled;
    private long _nextPublishSequenceNumber = 1;

    #region Properties

    public TimeSpan ContinuationTimeout { get; set; }
    public ulong NextPublishSeqNo => (ulong)_nextPublishSequenceNumber;
    public bool IsOpen { get; set; } = true;
    public bool IsClosed { get; set; }
    public ShutdownEventArgs? CloseReason { get; set; }
    public IAsyncBasicConsumer? DefaultConsumer { get; set; }
    public int ChannelNumber { get; }
    public string? CurrentQueue { get; }

    #endregion Properties

    #region Event Handlers

    public event AsyncEventHandler<ShutdownEventArgs>? ChannelShutdownAsync;
    public event AsyncEventHandler<CallbackExceptionEventArgs>? CallbackExceptionAsync;
    public event AsyncEventHandler<FlowControlEventArgs>? FlowControlAsync;
    public event AsyncEventHandler<BasicReturnEventArgs>? BasicReturnAsync;
    public event AsyncEventHandler<AsyncEventArgs>? BasicRecoverOkAsync;
    public event AsyncEventHandler<BasicNackEventArgs>? BasicNacksAsync;
    public event AsyncEventHandler<BasicAckEventArgs>? BasicAcksAsync;

    #endregion Event Handlers

    #region IChannel Implementation


    public Task AbortAsync(CancellationToken cancellationToken = default)
    {
        return AbortAsync(ushort.MaxValue, string.Empty, cancellationToken);
    }

    public Task AbortAsync(ushort replyCode, string replyText, CancellationToken cancellationToken = default)
    {
        IsClosed = true;
        IsOpen = false;
        CloseReason = new ShutdownEventArgs(ShutdownInitiator.Library, replyCode, replyText);
        return Task.CompletedTask;
    }

    public ValueTask BasicAckAsync(ulong deliveryTag, bool multiple, CancellationToken cancellationToken = default)
    {
        if (multiple)
        {
            while (BasicAckSingle(deliveryTag))
                --deliveryTag;
        }
        else
        {
            BasicAckSingle(deliveryTag);
        }
        return ValueTask.CompletedTask;
    }

    public Task BasicCancelAsync(string consumerTag, bool noWait = false, CancellationToken cancellationToken = default)
    {
        _consumers.TryRemove(consumerTag, out var consumer);

        if (consumer != null && !string.IsNullOrWhiteSpace(CurrentQueue))
        {
            _server.Queues.TryGetValue(CurrentQueue, out var queueInstance);
            queueInstance?.RemoveConsumer(consumerTag);
        }

        return Task.CompletedTask;
    }

    public Task<string> BasicConsumeAsync(string queue, bool autoAck, IAsyncBasicConsumer consumer, CancellationToken cancellationToken = default)
    {
        return BasicConsumeAsync(queue, autoAck, string.Empty, false, false, null, consumer, cancellationToken);
    }

    public Task<string> BasicConsumeAsync(string queue, bool autoAck, string consumerTag, IAsyncBasicConsumer consumer, CancellationToken cancellationToken = default)
    {
        return BasicConsumeAsync(queue, autoAck, consumerTag, false, false, null, consumer, cancellationToken);
    }

    public Task<string> BasicConsumeAsync(string queue, bool autoAck, string consumerTag, bool noLocal, bool exclusive, IDictionary<string, object?>? arguments, IAsyncBasicConsumer consumer, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(consumerTag))
        {
            consumerTag = Guid.NewGuid().ToString();
        }

        _consumers[consumerTag] = consumer;

        _server.Queues.TryGetValue(queue, out var queueInstance);
        queueInstance?.AddConsumer(consumerTag, consumer);

        return Task.FromResult(consumerTag);
    }

    public Task<BasicGetResult?> BasicGetAsync(string queue, bool autoAck, CancellationToken cancellationToken = default)
    {
        _server.Queues.TryGetValue(queue, out var queueInstance);

        if (queueInstance?.Messages.TryDequeue(out var message) == true)
        {
            var deliveryTag = (ulong)Interlocked.Increment(ref _lastDeliveryTag);
            message.DeliveryTag = deliveryTag;

            if (!autoAck)
            {
                WorkingMessages[deliveryTag] = message;
            }

            return Task.FromResult<BasicGetResult?>(new BasicGetResult(deliveryTag, false, message.Exchange, message.RoutingKey, (uint)queueInstance.Messages.Count, message.BasicProperties, message.Body));
        }

        return Task.FromResult<BasicGetResult?>(null);
    }

    public ValueTask BasicNackAsync(ulong deliveryTag, bool multiple, bool requeue, CancellationToken cancellationToken = default)
    {
        if (requeue) return ValueTask.CompletedTask;

        foreach (var queue in WorkingMessages.Select(m => m.Value.Queue))
        {
            _server.Queues.TryGetValue(queue!, out var queueInstance);

            if (queueInstance != null)
            {
                queueInstance.Messages = new ConcurrentQueue<RabbitMessage>();
            }
        }

        WorkingMessages.TryRemove(deliveryTag, out var message);
        if (message == null) return ValueTask.CompletedTask;

        _server.Queues.TryGetValue(message.Queue!, out var processingQueue);
        if (processingQueue?.Arguments != null
            && processingQueue.Arguments.TryGetValue("x-dead-letter-exchange", out var dlx)
            && _server.Exchanges.TryGetValue((string)dlx, out var exchange))
        {
            message.RoutingKey = processingQueue.Arguments.TryGetValue("x-dead-letter-routing-key", out var key)
                ? (string)key
                : message.Queue!;

            exchange.PublishMessage(message);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask BasicPublishAsync<TProperties>(string exchange, string routingKey, bool mandatory, TProperties basicProperties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default) where TProperties : IReadOnlyBasicProperties, IAmqpHeader
    {
        BasicPublishInternal(exchange, routingKey, mandatory, basicProperties as IBasicProperties, body.ToArray());
        return ValueTask.CompletedTask;
    }

    public ValueTask BasicPublishAsync(string exchange, string routingKey, bool mandatory, IBasicProperties? basicProperties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default)
    {
        BasicPublishInternal(exchange, routingKey, mandatory, basicProperties, body.ToArray());
        return ValueTask.CompletedTask;
    }

    public ValueTask BasicPublishAsync<TProperties>(CachedString exchange, CachedString routingKey, bool mandatory, TProperties basicProperties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default) where TProperties : IReadOnlyBasicProperties, IAmqpHeader
    {
        return BasicPublishAsync(exchange.Value, routingKey.Value, mandatory, basicProperties, body, cancellationToken);
    }

    public Task BasicQosAsync(uint prefetchSize, ushort prefetchCount, bool global, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task BasicRecoverAsync(bool requeue, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask BasicRejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default)
    {
        return BasicNackAsync(deliveryTag, false, requeue, cancellationToken);
    }

    public Task CloseAsync(CancellationToken cancellationToken = default)
    {
        return CloseAsync(ushort.MaxValue, string.Empty, false, cancellationToken);
    }

    public Task CloseAsync(ushort replyCode, string replyText, bool abort, CancellationToken cancellationToken = default)
    {
        return CloseAsync(new ShutdownEventArgs(ShutdownInitiator.Library, replyCode, replyText), abort, cancellationToken);
    }

    public Task CloseAsync(ShutdownEventArgs reason, bool abort, CancellationToken cancellationToken = default)
    {
        IsClosed = true;
        IsOpen = false;
        CloseReason = reason;
        return Task.CompletedTask;
    }

    public Task ConfirmSelectAsync(CancellationToken cancellationToken = default)
    {
        _publisherConfirmsEnabled = true;
        return Task.CompletedTask;
    }

    public Task<uint> ConsumerCountAsync(string queue, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IBasicProperties CreateBasicProperties()
    {
        return new FakeBasicProperties();
    }

    public Task ExchangeBindAsync(string destination, string source, string routingKey, IDictionary<string, object?>? arguments = null, bool noWait = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ExchangeDeclareAsync(string exchange, string type, bool durable = false, bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool noWait = false, bool passive = false, CancellationToken cancellationToken = default)
    {
        if (passive)
        {
            if (!_server.Exchanges.ContainsKey(exchange))
            {
                throw new InvalidOperationException($"Exchange '{exchange}' does not exist.");
            }
        }
        else
        {
            var exchangeInstance = ExchangeFactory.GetExchange(exchange, type);
            exchangeInstance.IsDurable = durable;
            exchangeInstance.AutoDelete = autoDelete;

            Func<string, Exchange, Exchange> updateFunction = (name, existing) => existing;
            _server.Exchanges.AddOrUpdate(exchange, exchangeInstance, updateFunction);
        }

        return Task.CompletedTask;
    }

    public Task ExchangeDeclarePassiveAsync(string exchange, CancellationToken cancellationToken = default)
    {
        return ExchangeDeclareAsync(exchange, string.Empty, false, false, null, false, true, cancellationToken);
    }

    public Task ExchangeDeleteAsync(string exchange, bool ifUnused = false, bool noWait = false, CancellationToken cancellationToken = default)
    {
        _server.Exchanges.TryRemove(exchange, out var _);
        return Task.CompletedTask;
    }

    public Task ExchangeUnbindAsync(string destination, string source, string routingKey, IDictionary<string, object?>? arguments = null, bool noWait = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task QueueBindAsync(string queue, string exchange, string routingKey, IDictionary<string, object?>? arguments = null, bool noWait = false, CancellationToken cancellationToken = default)
    {
        if (!_server.Queues.TryGetValue(queue, out var queueInstance))
        {
            throw new InvalidOperationException($"Cannot bind queue '{queue}' to exchange '{exchange}' because the specified queue does not exist.");
        }
        if (!_server.Exchanges.TryGetValue(exchange, out var exchangeInstance))
        {
            throw new InvalidOperationException($"Cannot bind queue '{queue}' to exchange '{exchange}' because the specified exchange does not exist.");
        }

        exchangeInstance.BindQueue(routingKey, queueInstance);
        return Task.CompletedTask;
    }

    public Task<QueueDeclareOk> QueueDeclareAsync(string queue, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool noWait = false, bool passive = false, CancellationToken cancellationToken = default)
    {
        var queueInstance = new Queue
        {
            Name = queue,
            IsDurable = durable,
            IsExclusive = exclusive,
            IsAutoDelete = autoDelete,
            Arguments = (arguments ?? new Dictionary<string, object?>()) as IDictionary<string, object> ?? new Dictionary<string, object>()
        };

        Func<string, Queue, Queue> updateFunction = (name, existing) => existing;
        _server.Queues.AddOrUpdate(queue, queueInstance, updateFunction);

        _server.DefaultExchange.BindQueue(queueInstance.Name, queueInstance);

        return Task.FromResult(new QueueDeclareOk(queue, 0, 0));
    }

    public Task<QueueDeclareOk> QueueDeclareAsync(CancellationToken cancellationToken = default)
    {
        var queueName = Guid.NewGuid().ToString();
        return QueueDeclareAsync(queueName, false, false, false, null, false, false, cancellationToken);
    }

    public Task<QueueDeclareOk> QueueDeclarePassiveAsync(string queue, CancellationToken cancellationToken = default)
    {
        return QueueDeclareAsync(queue, false, false, false, null, false, true, cancellationToken);
    }

    public Task<uint> QueueDeleteAsync(string queue, bool ifUnused = false, bool ifEmpty = false, bool noWait = false, CancellationToken cancellationToken = default)
    {
        _server.Queues.TryRemove(queue, out var instance);
        return Task.FromResult(instance != null ? 1u : 0u);
    }

    public Task<uint> QueuePurgeAsync(string queue, CancellationToken cancellationToken = default)
    {
        _server.Queues.TryGetValue(queue, out var instance);

        if (instance == null)
            return Task.FromResult(0u);

        while (!instance.Messages.IsEmpty)
        {
            instance.Messages.TryDequeue(out var _);
        }

        return Task.FromResult(1u);
    }

    public Task QueueUnbindAsync(string queue, string exchange, string routingKey, IDictionary<string, object?>? arguments = null, CancellationToken cancellationToken = default)
    {
        if (!_server.Queues.TryGetValue(queue, out var queueInstance))
        {
            throw new InvalidOperationException($"Cannot unbind queue '{queue}' from exchange '{exchange}' because the specified queue does not exist.");
        }
        if (!_server.Exchanges.TryGetValue(exchange, out var exchangeInstance))
        {
            throw new InvalidOperationException($"Cannot unbind queue '{queue}' from exchange '{exchange}' because the specified exchange does not exist.");
        }

        exchangeInstance.UnbindQueue(routingKey, queueInstance);
        return Task.CompletedTask;
    }

    public Task TxCommitAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task TxRollbackAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task TxSelectAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task<uint> MessageCountAsync(string queue, CancellationToken cancellationToken = default)
    {
        _server.Queues.TryGetValue(queue, out var queueInstance);
        return Task.FromResult(queueInstance != null ? (uint)queueInstance.Messages.Count : 0u);
    }

    public ValueTask<ulong> GetNextPublishSequenceNumberAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult((ulong)_nextPublishSequenceNumber);
    }

    public void Dispose()
    {
        // No cleanup needed for fake implementation
    }

    #endregion IChannel Implementation

    #region Additional Methods

    public IEnumerable<RabbitMessage> GetMessagesOnQueue(string queueName)
    {
        _server.Queues.TryGetValue(queueName, out var queueInstance);

        return queueInstance is not null ? (IEnumerable<RabbitMessage>)queueInstance.Messages : [];
    }

    #endregion Additional Methods

    #region Private Methods

    private void BasicPublishInternal(string exchange, string routingKey, bool mandatory, IBasicProperties? basicProperties, byte[] body)
    {
        var deliveryTag = (ulong)Interlocked.Increment(ref _lastDeliveryTag);
        var message = new RabbitMessage
        {
            Exchange = exchange,
            RoutingKey = routingKey,
            Mandatory = mandatory,
            Immediate = false,
            BasicProperties = basicProperties!,
            Body = body,
            DeliveryTag = deliveryTag
        };
        
        static RabbitMessage UpdateFunction(ulong key, RabbitMessage existingMessage) => existingMessage;
        WorkingMessages.AddOrUpdate(deliveryTag, message, UpdateFunction);

        if (!_server.Exchanges.TryGetValue(exchange, out var exchangeInstance))
        {
            throw new InvalidOperationException($"Cannot publish to exchange '{exchange}' as it does not exist.");
        }

        var canRoute = exchangeInstance.PublishMessage(message);

        Interlocked.Increment(ref _nextPublishSequenceNumber);

        if (_publisherConfirmsEnabled)
        {
            if (message.Mandatory && !canRoute)
            {
                _ = OnMessageReturnedAsync(message, exchange, routingKey);
            }

            _ = OnMessageAcknowledgedAsync(message);
        }
    }

    private bool BasicAckSingle(ulong deliveryTag)
    {
        WorkingMessages.TryRemove(deliveryTag, out var message);
        if (message == null) return false;

        _ = OnMessageAcknowledgedAsync(message);
        return true;
    }

    private async Task OnMessageAcknowledgedAsync(RabbitMessage message)
    {
        if (BasicAcksAsync != null)
        {
            var args = new BasicAckEventArgs(message.DeliveryTag, false);
            await BasicAcksAsync.Invoke(this, args);
        }
    }

    private async Task OnMessageReturnedAsync(RabbitMessage message, string exchange, string routingKey)
    {
        if (BasicReturnAsync != null)
        {
            var args = new BasicReturnEventArgs(404, "NO_ROUTE", exchange, routingKey, message.BasicProperties, message.Body);
            await BasicReturnAsync.Invoke(this, args);
        }
    }

    #endregion Private Methods
}