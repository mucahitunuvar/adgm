using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

// §4.2 "upcoming-events türleri SupportsEvent olmalı" - like content-list's category check, this
// needs a ContentType lookup and so belongs to PageLayoutReferenceValidator, not here.
public sealed class UpcomingEventsBlockTypeDefinition : BlockTypeDefinition<UpcomingEventsBlockSettings, UpcomingEventsBlockTexts>
{
    public override string Key => "upcoming-events";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(UpcomingEventsBlockSettings settings)
    {
        if (settings.ContentTypeKeys.Count == 0 || settings.ContentTypeKeys.Any(string.IsNullOrWhiteSpace))
        {
            return Result.Failure(Error.Validation(
                "upcoming-events.ContentTypeKeysRequired", "At least one non-empty content type key is required."));
        }

        if (settings.Count is < 1 or > 12)
        {
            return Result.Failure(Error.Validation("upcoming-events.CountInvalid", "Count must be between 1 and 12."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(UpcomingEventsBlockSettings settings, UpcomingEventsBlockTexts texts)
    {
        if (string.IsNullOrWhiteSpace(texts.Title))
        {
            return Result.Failure(Error.Validation("upcoming-events.TitleRequired", "A title is required."));
        }

        return Result.Success();
    }

    protected override BlockReferenceSet GetReferences(UpcomingEventsBlockSettings settings, IReadOnlyList<UpcomingEventsBlockTexts> texts) =>
        BlockReferenceSet.Empty with { EventContentTypeKeys = settings.ContentTypeKeys.Distinct(StringComparer.Ordinal).ToList() };
}
