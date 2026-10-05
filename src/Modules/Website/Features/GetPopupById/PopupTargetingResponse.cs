namespace GenclikMerkezi.Modules.Website.Features.GetPopupById;

public sealed record PopupTargetingResponse(string Kind, IReadOnlyList<Guid> ContentItemIds, IReadOnlyList<string> Paths);
