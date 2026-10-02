using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class FaqBlockTypeDefinition : BlockTypeDefinition<FaqBlockSettings, FaqBlockTexts>
{
    public override string Key => "faq";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(FaqBlockSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ContentTypeKey))
        {
            return Result.Failure(Error.Validation("faq.ContentTypeKeyRequired", "A content type key is required."));
        }

        if (settings.Count is < 1 or > 30)
        {
            return Result.Failure(Error.Validation("faq.CountInvalid", "Count must be between 1 and 30."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(FaqBlockSettings settings, FaqBlockTexts texts) => Result.Success();

    protected override BlockReferenceSet GetReferences(FaqBlockSettings settings, IReadOnlyList<FaqBlockTexts> texts) =>
        BlockReferenceSet.Empty with { ContentTypeReferences = [new ContentTypeCategoryReference(settings.ContentTypeKey, settings.CategoryId)] };
}
