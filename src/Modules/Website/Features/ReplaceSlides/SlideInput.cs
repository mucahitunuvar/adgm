namespace GenclikMerkezi.Modules.Website.Features.ReplaceSlides;

public sealed record SlideInput(
    Guid DesktopImageMediaId,
    Guid? MobileImageMediaId,
    SlideLinkInput? Link,
    int SortOrder,
    bool IsActive,
    DateTime? PublishAtUtc,
    DateTime? UnpublishAtUtc,
    IReadOnlyList<SlideTranslationInput> Translations);
