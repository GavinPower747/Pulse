using RabbitMQ.Client.Events;

namespace Pulse.Shared.Messaging.Resiliance;

internal interface IFailureHandler
{
    public Task HandleFailure(
        BasicDeliverEventArgs args,
        IntegrationEvent evt,
        CancellationToken ct = default
    );
}
