using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class ProcessStepsBlockTypeDefinition : BlockTypeDefinition<ProcessStepsBlockSettings, ProcessStepsBlockTexts>
{
    public override string Key => "process-steps";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(ProcessStepsBlockSettings settings)
    {
        if (settings.StepCount is < 2 or > 8)
        {
            return Result.Failure(Error.Validation("process-steps.StepCountInvalid", "StepCount must be between 2 and 8."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(ProcessStepsBlockSettings settings, ProcessStepsBlockTexts texts)
    {
        if (string.IsNullOrWhiteSpace(texts.Title))
        {
            return Result.Failure(Error.Validation("process-steps.TitleRequired", "A title is required."));
        }

        if (texts.Steps.Count != settings.StepCount)
        {
            return Result.Failure(Error.Validation(
                "process-steps.StepCountMismatch", "process-steps' texts must have exactly StepCount steps."));
        }

        if (texts.Steps.Any(s => string.IsNullOrWhiteSpace(s.Title) || string.IsNullOrWhiteSpace(s.Text)))
        {
            return Result.Failure(Error.Validation("process-steps.StepTextRequired", "Every step requires a title and text."));
        }

        return Result.Success();
    }

    protected override BlockReferenceSet GetReferences(ProcessStepsBlockSettings settings, IReadOnlyList<ProcessStepsBlockTexts> texts) =>
        BlockReferenceSet.Empty;
}
