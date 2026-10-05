namespace GenclikMerkezi.Modules.Website.Application.Popups;

// Faz 2 Görev 6 master prompt §6 "Hedefleme": the JSON-serializable shape CreatePopup/UpdatePopup's
// request carries for Targeting, mirroring LinkTargetDto's own "one dto, several kinds" shape.
// PopupTargetingInputMapper turns this into the real domain PopupTargeting value object.
public sealed record PopupTargetingInput(string Kind, IReadOnlyList<Guid>? ContentItemIds, IReadOnlyList<string>? Paths);
