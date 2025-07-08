using RabbitMQ.Client;

namespace Pulse.Tests.Util.Mocks.RabbitMQ;

public class FakeConnectionFactory(RabbitServer server) : IConnectionFactory
{
    #region Properties

    #region IConnectionFactory Implementation

    public ushort RequestedChannelMax { get; set; }

    public string? ClientProvidedName { get; set; }

    public Uri Uri { get; set; } = new Uri("amqp://localhost:5672");

    public string UserName { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    public bool UseBackgroundThreadsForIO { get; set; }

    public TimeSpan RequestedHeartbeat { get; set; }

    public uint RequestedFrameMax { get; set; }

    public TimeSpan HandshakeContinuationTimeout { get; set; }

    public TimeSpan ContinuationTimeout { get; set; }

    public IDictionary<string, object?> ClientProperties { get; set; } = new Dictionary<string, object?>();

    public string Password { get; set; } = "guest";

    #endregion IConnectionFactory Implementation

    public IConnection Connection { get; private set; } = new FakeConnection(server);

    public RabbitServer Server { get; private set; } = server;

    public FakeConnection UnderlyingConnection
    {
        get { return (FakeConnection)Connection; }
    }

    public List<IChannel> UnderlyingModel
    {
        get => UnderlyingConnection is not null
            ? UnderlyingConnection.Channels 
            : [];
    }

    public ICredentialsProvider? CredentialsProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ushort ConsumerDispatchConcurrency { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    #endregion Properties

    #region Constructors


    public FakeConnectionFactory() : this(new RabbitServer())
    {
    }

    #endregion Constructors

    #region Public Methods

    #region IConnectionFactory Implementation


    public IAuthMechanismFactory AuthMechanismFactory(IList<string> mechanismNames)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints, string clientProvidedName)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection(IList<string> hostnames, string clientProvidedName)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection(IList<string> hostnames)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection(string clientProvidedName)
    {
        throw new NotImplementedException();
    }

    public IConnection CreateConnection()
    {
        if (Connection == null)
        {
            Connection = new FakeConnection(Server);
        }

        return Connection;
    }

    #endregion IConnectionFactory Implementation

    public FakeConnectionFactory WithConnection(IConnection connection)
    {
        Connection = connection;
        return this;
    }

    public FakeConnectionFactory WithRabbitServer(RabbitServer server)
    {
        Server = server;
        return this;
    }

    public IAuthMechanismFactory? AuthMechanismFactory(IEnumerable<string> mechanismNames)
    {
        throw new NotImplementedException();
    }

    public Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IConnection> CreateConnectionAsync(string clientProvidedName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IConnection> CreateConnectionAsync(IEnumerable<string> hostnames, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IConnection> CreateConnectionAsync(IEnumerable<string> hostnames, string clientProvidedName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IConnection> CreateConnectionAsync(IEnumerable<AmqpTcpEndpoint> endpoints, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IConnection> CreateConnectionAsync(IEnumerable<AmqpTcpEndpoint> endpoints, string clientProvidedName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }


    #endregion Public Methods
}