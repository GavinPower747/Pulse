namespace Pulse.Shared.Messaging;

public interface IProducer
{
    Task Publish(IntegrationEvent evt, CancellationToken token = default);
}
