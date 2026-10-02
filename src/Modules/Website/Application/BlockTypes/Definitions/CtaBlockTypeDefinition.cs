using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class CtaBlockTypeDefinition : BlockTypeDefinition<CtaBlockSettings, CtaBlockTexts>
{
    private static readonly string[] AllowedStyles = ["primary", "secondary", "quiet"];

    public override string Key => "cta";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(CtaBlockSettings settings)
    {
        if (settings.Buttons.Count is < 1 or > 3)
        {
            return Result.Failure(Error.Validation("cta.ButtonCountInvalid", "cta must have between 1 and 3 buttons."));
        }

        foreach (var button in settings.Buttons)
        {
            if (!AllowedStyles.Contains(button.Style, StringComparer.Ordinal))
            {
                return Result.Failure(Error.Validation("cta.StyleInvalid", "Style must be 'primary', 'secondary' or 'quiet'."));
            }

            var linkResult = LinkTargetDtoMapper.ToLinkTarget(button.Link);
            if (linkResult.IsFailure)
            {
                return Result.Failure(linkResult.Error);
            }

            if (linkResult.Value.IsEmpty)
            {
                return Result.Failure(Error.Validation("cta.LinkRequired", "Each cta button requires a link."));
            }
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(CtaBlockSettings settings, CtaBlockTexts texts)
    {
        if (string.IsNullOrWhiteSpace(texts.Title))
        {
            return Result.Failure(Error.Validation("cta.TitleRequired", "A title is required."));
        }

        if (texts.Buttons.Count != settings.Buttons.Count)
        {
            return Result.Failure(Error.Validation("cta.ButtonCountMismatch", "cta's texts must have the same number of buttons as its settings."));
        }

        if (texts.Buttons.Any(b => string.IsNullOrWhiteSpace(b.Label)))
        {
            return Result.Failure(Error.Validation("cta.ButtonLabelRequired", "Every button requires a label."));
        }

        return Result.Success();
    }

    protected override BlockReferenceSet GetReferences(CtaBlockSettings settings, IReadOnlyList<CtaBlockTexts> texts)
    {
        var contentItemIds = new List<Guid>();
        var contentTypeListingIds = new List<Guid>();

        foreach (var button in settings.Buttons)
        {
            LinkTargetDtoMapper.AppendReferences(button.Link, contentItemIds, contentTypeListingIds);
        }

        return BlockReferenceSet.Empty with { ContentItemIds = contentItemIds, ContentTypeListingIds = contentTypeListingIds };
    }
}
