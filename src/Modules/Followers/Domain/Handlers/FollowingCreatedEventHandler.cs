using MediatR;
using Pulse.Followers.Contracts.Events;
using Pulse.Followers.Events;
using Pulse.Shared.Messaging;

namespace Pulse.Followers.Domain.Handlers;

internal class FollowingCreatedEventHandler(IProducer messageBus)
    : INotificationHandler<FollowingCreatedEvent>
{
    private readonly IProducer _messageBus = messageBus;

    public Task Handle(FollowingCreatedEvent notification, CancellationToken cancellationToken)
    {
        var message = new UserFollowedEvent(notification.FollowerId, notification.FollowingId);

        return _messageBus.Publish(message, cancellationToken);
    }
}
