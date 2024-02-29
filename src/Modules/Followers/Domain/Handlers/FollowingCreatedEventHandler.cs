using MassTransit;
using MediatR;
using Pulse.Followers.Contracts.Events;
using Pulse.Followers.Events;

namespace Pulse.Followers.Domain.Handlers;

internal class FollowingCreatedEventHandler(IBus messageBus)
    : INotificationHandler<FollowingCreatedEvent>
{
    private readonly IBus _messageBus = messageBus;

    public Task Handle(FollowingCreatedEvent notification, CancellationToken cancellationToken)
    {
        var message = new UserFollowedEvent(notification.FollowerId, notification.FollowingId);

        return _messageBus.Publish(message, cancellationToken);
    }
}
