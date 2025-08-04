using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Data;
using Pulse.Shared.Messaging;
using Pulse.Users.Contracts.Messages;

namespace Pulse.Posts.Consumers;

internal class MentionValidatedConsumer(PostsContext dbContext) : IConsumer<MentionValidatedEvent>
{
    private readonly PostsContext _dbContext = dbContext;

    public async Task Consume(MentionValidatedEvent evt, CancellationToken token = default)
    {
        var post = await _dbContext.PostSet.FirstOrDefaultAsync(p => p.Id == evt.PostId, token);

        if (post is null)
            return;

        post.AddMention(evt.Username);

        _dbContext.PostSet.Update(post);
        await _dbContext.SaveChangesAsync(token);
    }
}
