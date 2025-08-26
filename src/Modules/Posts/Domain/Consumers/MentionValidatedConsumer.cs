using Microsoft.EntityFrameworkCore;
using Pulse.Posts.Data;
using Pulse.Shared.Messaging;
using Pulse.Users.Contracts.Messages;

namespace Pulse.Posts.Consumers;

internal class MentionValidatedConsumer(IDbContextFactory<PostsContext> dbContextFactory)
    : IConsumer<MentionValidatedEvent>
{
    private readonly IDbContextFactory<PostsContext> _dbContextFactory = dbContextFactory;

    public async Task Consume(MentionValidatedEvent evt, CancellationToken token = default)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync(token);
        var post = await context.PostSet.FirstOrDefaultAsync(p => p.Id == evt.PostId, token);

        if (post is null)
            return;

        post.AddMention(evt.Username);

        context.PostSet.Update(post);
        await context.SaveChangesAsync(token);
    }
}
