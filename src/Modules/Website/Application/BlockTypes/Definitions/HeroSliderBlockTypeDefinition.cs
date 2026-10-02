using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

// §4.2: the one block type with no per-language texts at all ("Metinler: —").
public sealed class HeroSliderBlockTypeDefinition : BlockTypeDefinition<HeroSliderBlockSettings, EmptyBlockTexts>
{
    public override string Key => "hero-slider";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(HeroSliderBlockSettings settings)
    {
        if (settings.SliderId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("hero-slider.SliderIdRequired", "A slider id is required."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(HeroSliderBlockSettings settings, EmptyBlockTexts texts) => Result.Success();

    protected override BlockReferenceSet GetReferences(HeroSliderBlockSettings settings, IReadOnlyList<EmptyBlockTexts> texts) =>
        BlockReferenceSet.Empty with { SliderIds = [settings.SliderId] };
}
