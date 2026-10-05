namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// Faz 2 Görev 6 master prompt §6: "Hangi pop-up'ın gösterileceğine frontend karar verir" - every
// currently visible, translated popup is returned, Priority descending; the frontend picks the
// highest-priority Modal and Banner whose Targeting matches the current page and DeviceTarget matches
// the current device.
public sealed record PublicPopupResponse(
    Guid Id,
    string DisplayMode,
    string? Title,
    string Body,
    string? ButtonLabel,
    string? Href,
    PublicPopupImageResponse? Image,
    string DeviceTarget,
    PublicPopupTargetingResponse Targeting,
    int DelaySeconds,
    string Frequency,
    int? FrequencyDays,
    bool Dismissible,
    int Priority);
