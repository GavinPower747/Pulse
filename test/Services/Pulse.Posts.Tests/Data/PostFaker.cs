using Bogus;
using Pulse.Posts.Domain;

namespace Pulse.Posts.Tests.Data;

public class PostFaker : Faker<Post>
{
    public PostFaker()
    {
        CustomInstantiator(f => new Post(
            f.Random.Guid(),
            f.Random.Guid(),
            f.Rant.Review(),
            f.Date.Past(),
            f.Date.Future(),
            f.Date.Future()
        ));
    }

    public Faker<Post> ForUser(Guid userId)
       => CustomInstantiator(f => new Post(
           f.Random.Guid(),
           userId,
           f.Rant.Review(),
           f.Date.Past(),
           f.Date.Future(),
           f.Date.Future()
       ));
}