namespace Pulse.Shared.Messaging;

public interface IConsumer<T> where T : IntegrationEvent
{
    Task Consume(T evt, CancellationToken token = default);
}
