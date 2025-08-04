namespace Pulse.Shared.Messaging;

public interface IConsumer<T> : IConsumer
    where T : IntegrationEvent
{
    Task Consume(T evt, CancellationToken token = default);
}

/// <summary>
/// Non-generic version so that we can use a list of IConsumer.
/// </summary>
public interface IConsumer { }
