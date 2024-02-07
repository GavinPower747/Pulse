using Mapster;
using Pulse.Posts.Contracts;
using Pulse.Users.Contracts;
using Pulse.WebApp.Features.Posts.Models;

namespace Pulse.WebApp.Features.Posts.Mapping;

public class MappingConfig
{
    public MappingConfig()
    {
        TypeAdapterConfig<DisplayPost, PostViewModel>.NewConfig();
        TypeAdapterConfig<User, PostViewModel>.NewConfig();
    }
}

public class PostMapper
{
    public PostViewModel MapToViewModel(DisplayPost post, User user)
    {
        var viewModel = new PostViewModel();
        post.Adapt(viewModel);
        user.Adapt(viewModel);

        return viewModel;
    }
}
