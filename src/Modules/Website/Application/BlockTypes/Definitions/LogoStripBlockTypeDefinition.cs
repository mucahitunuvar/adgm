using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class LogoStripBlockTypeDefinition : BlockTypeDefinition<LogoStripBlockSettings, LogoStripBlockTexts>
{
    public override string Key => "logo-strip";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(LogoStripBlockSettings settings)
    {
        if (settings.MaxItems < 1 || settings.MaxItems > 30)
        {
            return Result.Failure(Error.Validation("logo-strip.MaxItemsInvalid", "MaxItems must be between 1 and 30."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(LogoStripBlockSettings settings, LogoStripBlockTexts texts)
    {
        if (texts.Title is { Length: > 150 })
        {
            return Result.Failure(Error.Validation("logo-strip.TitleTooLong", "Title must be at most 150 characters."));
        }

        return Result.Success();
    }

    protected override BlockReferenceSet GetReferences(LogoStripBlockSettings settings, IReadOnlyList<LogoStripBlockTexts> texts) =>
        BlockReferenceSet.Empty;
}
