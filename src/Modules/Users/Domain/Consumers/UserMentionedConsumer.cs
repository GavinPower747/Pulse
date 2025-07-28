using Pulse.Posts.Contracts.Messages;
using Pulse.Shared.Messaging;
using Pulse.Users.Contracts;
using Pulse.Users.Contracts.Messages;

namespace Pulse.Users.Consumers;

internal class UserMentionedConsumer(IProducer producer, IUserQueries userQueries) : IConsumer<UserMentionedEvent>
{
    private readonly IProducer _producer = producer;
    private readonly IUserQueries _userQueries = userQueries;

    public async Task Consume(UserMentionedEvent evt, CancellationToken token = default)
    {
        var userExists = await _userQueries.UserExists(evt.Username);

        if (!userExists)
            return;

        await _producer.Publish(new MentionValidatedEvent(evt.PostId, evt.Username), token);
    }
}
