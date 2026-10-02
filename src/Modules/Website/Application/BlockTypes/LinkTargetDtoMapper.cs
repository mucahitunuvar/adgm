using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// Mirrors ReplaceSlidesCommandHandler/ReplaceMenuItemsCommandHandler's own private BuildLinkTarget -
// block type definitions need the identical dto-to-domain-VO conversion for every LinkTargetDto field
// in their Settings/Texts records.
internal static class LinkTargetDtoMapper
{
    public static Result<LinkTarget> ToLinkTarget(LinkTargetDto? dto)
    {
        if (dto is null)
        {
            return LinkTarget.CreateEmpty();
        }

        if (!Enum.TryParse<LinkTargetKind>(dto.Kind, ignoreCase: true, out var kind))
        {
            return Result.Failure<LinkTarget>(Error.Validation("BlockType.LinkKindInvalid", $"Unknown link kind '{dto.Kind}'."));
        }

        return kind switch
        {
            LinkTargetKind.None => LinkTarget.CreateEmpty(),
            LinkTargetKind.Content => LinkTarget.ForContent(dto.ContentItemId ?? Guid.Empty),
            LinkTargetKind.ContentTypeListing => LinkTarget.ForContentTypeListing(dto.ContentTypeId ?? Guid.Empty),
            LinkTargetKind.InternalPath => LinkTarget.ForInternalPath(dto.InternalPath),
            LinkTargetKind.ExternalUrl => LinkTarget.ForExternalUrl(dto.ExternalUrl),
            _ => Result.Failure<LinkTarget>(Error.Validation("BlockType.LinkKindInvalid", $"Unknown link kind '{dto.Kind}'.")),
        };
    }

    // GetReferences implementations call this for every LinkTargetDto field they carry - a Content or
    // ContentTypeListing link target is itself a reference PageLayoutReferenceValidator must confirm
    // still exists (§4.1 "içerik ID'leri" / "içerik türü"). A dto whose shape is already invalid (it
    // would have failed ValidateSettings/ValidateTexts before GetReferences ever runs) or that targets
    // an internal path/external URL/nothing simply contributes no reference.
    public static void AppendReferences(LinkTargetDto? dto, List<Guid> contentItemIds, List<Guid> contentTypeListingIds)
    {
        var result = ToLinkTarget(dto);
        if (result.IsFailure)
        {
            return;
        }

        var target = result.Value;
        if (target.Kind == LinkTargetKind.Content && target.ContentItemId is { } contentItemId)
        {
            contentItemIds.Add(contentItemId);
        }
        else if (target.Kind == LinkTargetKind.ContentTypeListing && target.ContentTypeId is { } contentTypeId)
        {
            contentTypeListingIds.Add(contentTypeId);
        }
    }
}
