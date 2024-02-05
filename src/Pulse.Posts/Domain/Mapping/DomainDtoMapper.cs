using Pulse.Posts.Contracts;

using Riok.Mapperly.Abstractions;

namespace Pulse.Posts.Domain.Mapping;

[Mapper]
internal partial class DomainDtoMapper
{
    public partial DisplayPost MapToDisplayPost(Post post);

    public partial Post? MapToPost(DisplayPost? displayPost);
}