namespace GenclikMerkezi.Modules.Website.Features.GetSliderById;

public sealed record SlideResponse(
    Guid Id,
    Guid DesktopImageMediaId,
    Guid? MobileImageMediaId,
    SlideLinkResponse? Link,
    int SortOrder,
    bool IsActive,
    DateTime? PublishAtUtc,
    DateTime? UnpublishAtUtc,
    IReadOnlyList<SlideTranslationResponse> Translations);
