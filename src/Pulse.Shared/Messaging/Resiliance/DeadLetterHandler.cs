

using RabbitMQ.Client.Events;

namespace Pulse.Shared.Messaging.Resiliance;

internal class DeadLetterHandler : IFailureHandler
{
    public Task HandleFailure(BasicDeliverEventArgs args, IntegrationEvent evt, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

}