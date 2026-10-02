namespace GenclikMerkezi.Modules.Website.Features.UpdateSlider;

public sealed record UpdateSliderRequest(byte[] RowVersion, IReadOnlyList<SliderTranslationInput> Translations);
