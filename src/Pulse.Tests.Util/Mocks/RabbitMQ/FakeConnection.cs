using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pulse.Tests.Util.Mocks.RabbitMQ;

public class FakeConnection(RabbitServer server) : IConnection
{
    private readonly RabbitServer _server = server;

    #region Properties

    #region INetworkConnection Implementation

    public int LocalPort { get; }

    public int RemotePort { get; }

    #endregion INetworkConnection Implementation

    #region IConnection Implementation

    public IDictionary<string, object?> ClientProperties { get; set; } =
        new Dictionary<string, object?>();

    public string? ClientProvidedName { get; } = null;

    public IDictionary<string, object?>? ServerProperties { get; set; }

    public IProtocol Protocol { get; set; } = default!;

    public AmqpTcpEndpoint[]? KnownHosts { get; set; }

    public bool IsOpen { get; set; }

    public TimeSpan Heartbeat { get; set; }

    public uint FrameMax { get; set; }

    public AmqpTcpEndpoint Endpoint { get; set; } = default!;

    public ushort ChannelMax { get; set; }

    public ShutdownEventArgs? CloseReason { get; set; }

    #endregion IConnection Implementation

    public List<IChannel> Channels { get; private set; } = [];

    public IEnumerable<ShutdownReportEntry> ShutdownReport => throw new NotImplementedException();

    #endregion Properties

    #region Event Handlers


    public event EventHandler<EventArgs>? RecoverySucceeded;

    public event EventHandler<ConnectionRecoveryErrorEventArgs>? ConnectionRecoveryError;

    public event EventHandler<ConnectionBlockedEventArgs>? ConnectionBlocked;

    public event EventHandler<EventArgs>? ConnectionUnblocked;
    public event AsyncEventHandler<CallbackExceptionEventArgs>? CallbackExceptionAsync;
    public event AsyncEventHandler<ShutdownEventArgs>? ConnectionShutdownAsync;
    public event AsyncEventHandler<AsyncEventArgs>? RecoverySucceededAsync;
    public event AsyncEventHandler<ConnectionRecoveryErrorEventArgs>? ConnectionRecoveryErrorAsync;
    public event AsyncEventHandler<ConsumerTagChangedAfterRecoveryEventArgs>? ConsumerTagChangeAfterRecoveryAsync;
    public event AsyncEventHandler<QueueNameChangedAfterRecoveryEventArgs>? QueueNameChangedAfterRecoveryAsync;
    public event AsyncEventHandler<RecoveringConsumerEventArgs>? RecoveringConsumerAsync;
    public event AsyncEventHandler<ConnectionBlockedEventArgs>? ConnectionBlockedAsync;
    public event AsyncEventHandler<AsyncEventArgs>? ConnectionUnblockedAsync;

    #endregion Event Handlers

    #region Public Methods

    #region IConnection Implementation


    public void Abort(ushort reasonCode, string reasonText, TimeSpan timeout)
    {
        IsOpen = false;
        CloseReason = new ShutdownEventArgs(ShutdownInitiator.Library, reasonCode, reasonText);

        Channels.ForEach(m => m.AbortAsync().Wait());
    }

    public void Abort(TimeSpan timeout)
    {
        Abort(1, string.Empty, timeout);
    }

    public void Abort(ushort reasonCode, string reasonText)
    {
        Abort(reasonCode, reasonText, TimeSpan.FromSeconds(0));
    }

    public void Abort()
    {
        Abort(1, string.Empty, TimeSpan.FromSeconds(0));
    }

    public void Close(ushort reasonCode, string reasonText, TimeSpan timeout)
    {
        IsOpen = false;
        CloseReason = new ShutdownEventArgs(ShutdownInitiator.Library, reasonCode, reasonText);

        Channels.ForEach(m => m.CloseAsync().Wait());
    }

    public void Close(TimeSpan timeout)
    {
        Close(1, string.Empty, timeout);
    }

    public void Close(ushort reasonCode, string reasonText)
    {
        Close(reasonCode, reasonText, TimeSpan.FromSeconds(0));
    }

    public void Close()
    {
        Close(1, string.Empty, TimeSpan.FromSeconds(0));
    }

    public IChannel CreateChannel()
    {
        var channel = new FakeChannel(_server);
        Channels.Add(channel);

        return channel;
    }

    public void HandleConnectionBlocked(string reason) { }

    public void HandleConnectionUnblocked() { }

    public void UpdateSecret(string newSecret, string reason)
    {
        throw new NotImplementedException();
    }

    #endregion IConnection Implementation

    #region IDisposable Implementation

    public void Dispose() { }

    public Task UpdateSecretAsync(
        string newSecret,
        string reason,
        CancellationToken cancellationToken = default
    ) => Task.Run(() => UpdateSecret(newSecret, reason), cancellationToken);

    public Task CloseAsync(
        ushort reasonCode,
        string reasonText,
        TimeSpan timeout,
        bool abort,
        CancellationToken cancellationToken = default
    ) => Task.Run(() => Close(reasonCode, reasonText, timeout), cancellationToken);

    public Task<IChannel> CreateChannelAsync(
        CreateChannelOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(CreateChannel());
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    #endregion IDisposable Implementation


    #endregion Public Methods
}
