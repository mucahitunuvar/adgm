namespace GenclikMerkezi.Modules.Website.Features.GetPopupById;

public sealed record PopupDetailResponse(
    Guid Id,
    string DisplayMode,
    Guid? ImageMediaId,
    PopupImageResponse? Image,
    PopupLinkResponse? Link,
    PopupTargetingResponse Targeting,
    string DeviceTarget,
    DateTime? PublishAtUtc,
    DateTime? UnpublishAtUtc,
    bool IsActive,
    int DelaySeconds,
    string Frequency,
    int? FrequencyDays,
    bool Dismissible,
    int Priority,
    byte[] RowVersion,
    IReadOnlyList<PopupTranslationResponse> Translations,
    DateTime CreatedAtUtc);
