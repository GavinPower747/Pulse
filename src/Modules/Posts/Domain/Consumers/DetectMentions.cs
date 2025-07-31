using System.Text.RegularExpressions;
using Pulse.Posts.Contracts.Messages;
using Pulse.Posts.Domain;
using Pulse.Shared.Messaging;

namespace Pulse.Posts.Consumers;

internal partial class DetectMentions(IProducer producer) : IConsumer<PostCreatedEvent>
{
    private readonly IProducer _producer = producer;

    public async Task Consume(PostCreatedEvent evt, CancellationToken token = default)
    {
        var post = new Post(
            evt.Id,
            Guid.Empty,
            evt.Content,
            DateTime.MinValue,
            DateTime.MinValue,
            null
        );
        var mentions = post.GetMentionedUsernames();

        if (!mentions.Any())
            return;

        foreach (var mention in mentions)
        {
            var userMentionEvent = new UserMentionedEvent(evt.Id, mention);
            await _producer.Publish(userMentionEvent, token);
        }
    }
}
