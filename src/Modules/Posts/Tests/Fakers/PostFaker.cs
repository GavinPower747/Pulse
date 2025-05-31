using Bogus;
using Pulse.Posts.Domain;

namespace Pulse.Posts.Tests.Fakers;

internal class PostFaker : Faker<Post>
{
    public PostFaker()
    {
        ReplaceConstructor(FakerHub.Random.Guid());
    }

    public Faker<Post> ForUser(Guid userId) => ReplaceConstructor(userId);

    public Faker<Post> ReplaceConstructor(Guid userId) =>
        CustomInstantiator(f => new Post
        {
            Id = f.Random.Guid(),
            UserId = userId,
            Content = f.Rant.Review(),
            CreatedAt = f.Date.Past(),
            PublishedAt = f.Date.Future()
        });
}
