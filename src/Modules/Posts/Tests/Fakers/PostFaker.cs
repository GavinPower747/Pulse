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
        CustomInstantiator(f => new Post(
            f.Random.Guid(),
            userId,
            f.Rant.Review(),
            f.Date.Past(),
            f.Date.Future()
        ));
}
