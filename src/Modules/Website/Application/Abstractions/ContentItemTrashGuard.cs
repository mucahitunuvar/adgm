using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.5 (Faz 1b Görev 6): a content item in the trash cannot be edited, published, or have any
// of its relations changed - only restored or permanently deleted. Checked once here, called by every
// command handler that mutates an existing ContentItem, instead of a near-identical inline check per
// handler (the same MediaImageReferenceGuard-style "one place, many callers" pattern).
public static class ContentItemTrashGuard
{
    public static Result EnsureEditable(ContentItem contentItem) =>
        contentItem.DeletedAtUtc is null
            ? Result.Success()
            : Result.Failure(Error.Conflict(
                "ContentItem.InTrash", "This content item is in the trash and cannot be modified; restore it first."));
}
