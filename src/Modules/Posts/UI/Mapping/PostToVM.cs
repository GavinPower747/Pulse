using Mapster;
using Pulse.Posts.Contracts;
using Pulse.Posts.UI.Models;
using Pulse.Users.Contracts;

namespace Pulse.Posts.UI.Mapping;

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

    public PostViewModel MapToViewModel(DisplayPost post, string username, string displayName)
    {
        var viewModel = new PostViewModel();
        post.Adapt(viewModel);

        viewModel.Username = username;
        viewModel.DisplayName = displayName;

        return viewModel;
    }
}
