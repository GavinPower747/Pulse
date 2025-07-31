using Pulse.Posts.Contracts;
using Riok.Mapperly.Abstractions;

namespace Pulse.Posts.Domain.Mapping;

[Mapper]
internal partial class DomainDtoMapper
{
    public partial DisplayPost MapToDisplayPost(Post post);

    public static AttachmentDownload MapToAttachmentDownload(AttachmentMetadata attachment)
    {
        return new AttachmentDownload(
            attachment.Id,
            Routes.GetAttachment(attachment.PostId, attachment.GetFileName()),
            attachment.ETag
        );
    }
}
