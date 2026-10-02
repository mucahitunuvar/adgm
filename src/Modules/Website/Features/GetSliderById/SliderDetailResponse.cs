namespace GenclikMerkezi.Modules.Website.Features.GetSliderById;

public sealed record SliderDetailResponse(
    Guid Id,
    string Key,
    byte[] RowVersion,
    IReadOnlyList<SliderTranslationResponse> Translations,
    IReadOnlyList<SlideResponse> Slides);
