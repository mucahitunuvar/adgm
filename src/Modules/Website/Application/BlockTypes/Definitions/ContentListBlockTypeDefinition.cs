using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

// §4.2's cross-field rule "content-list'teki kategori o türe ait olmalı" needs database access
// (the resolved ContentType and the category both need loading) and so cannot be checked here - it
// is PageLayoutReferenceValidator's job, driven by the ContentTypeCategoryReference this returns.
public sealed class ContentListBlockTypeDefinition : BlockTypeDefinition<ContentListBlockSettings, ContentListBlockTexts>
{
    private static readonly string[] AllowedViews = ["cards", "list"];

    public override string Key => "content-list";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(ContentListBlockSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ContentTypeKey))
        {
            return Result.Failure(Error.Validation("content-list.ContentTypeKeyRequired", "A content type key is required."));
        }

        if (settings.Count is < 1 or > 12)
        {
            return Result.Failure(Error.Validation("content-list.CountInvalid", "Count must be between 1 and 12."));
        }

        if (!AllowedViews.Contains(settings.View, StringComparer.Ordinal))
        {
            return Result.Failure(Error.Validation("content-list.ViewInvalid", "View must be 'cards' or 'list'."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(ContentListBlockSettings settings, ContentListBlockTexts texts)
    {
        if (string.IsNullOrWhiteSpace(texts.Title))
        {
            return Result.Failure(Error.Validation("content-list.TitleRequired", "A title is required."));
        }

        return Result.Success();
    }

    protected override BlockReferenceSet GetReferences(ContentListBlockSettings settings, IReadOnlyList<ContentListBlockTexts> texts) =>
        BlockReferenceSet.Empty with { ContentTypeReferences = [new ContentTypeCategoryReference(settings.ContentTypeKey, settings.CategoryId)] };
}
