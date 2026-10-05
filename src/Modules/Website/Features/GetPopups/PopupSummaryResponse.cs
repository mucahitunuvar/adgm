namespace GenclikMerkezi.Modules.Website.Features.GetPopups;

public sealed record PopupSummaryResponse(
    Guid Id,
    string DisplayMode,
    string? Title,
    bool IsActive,
    int Priority,
    DateTime? PublishAtUtc,
    DateTime? UnpublishAtUtc,
    byte[] RowVersion);
