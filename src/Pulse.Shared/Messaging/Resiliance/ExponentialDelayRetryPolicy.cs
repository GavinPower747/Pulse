using RabbitMQ.Client.Events;

namespace Pulse.Shared.Messaging.Resiliance;

internal class ExponentialDelayRetryHandler : IFailureHandler
{
    public Task HandleFailure(BasicDeliverEventArgs args, IntegrationEvent evt, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

}
