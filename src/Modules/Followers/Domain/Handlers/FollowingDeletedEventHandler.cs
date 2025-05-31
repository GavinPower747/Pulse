using MediatR;
using Pulse.Followers.Contracts.Events;
using Pulse.Shared.Messaging;

namespace Pulse.Followers;

internal class FollowingDeletedEventHandler(IProducer messageBus)
    : INotificationHandler<FollowingDeletedEvent>
{
    private readonly IProducer _messageBus = messageBus;

    public Task Handle(FollowingDeletedEvent notification, CancellationToken cancellationToken)
    {
        var message = new UserUnfollowedEvent(notification.UserId, notification.FollowerId);

        return _messageBus.Publish(message, cancellationToken);
    }
}
