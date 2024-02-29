using MassTransit;
using MediatR;
using Pulse.Followers.Contracts.Events;

namespace Pulse.Followers;

internal class FollowingDeletedEventHandler(IBus messageBus)
    : INotificationHandler<FollowingDeletedEvent>
{
    private readonly IBus _messageBus = messageBus;

    public Task Handle(FollowingDeletedEvent notification, CancellationToken cancellationToken)
    {
        var message = new UserUnfollowedEvent(notification.UserId, notification.FollowerId);

        return _messageBus.Publish(message, cancellationToken);
    }
}
