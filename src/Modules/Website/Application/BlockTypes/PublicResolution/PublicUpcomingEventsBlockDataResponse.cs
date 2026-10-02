namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: upcoming-events' `data` - visible items from every selected
// content type, merged and capped at Count (Faz 1b's temporary EventDateAsc-as-PublishDateDesc
// behavior, see ContentTypeSortMode's remarks).
public sealed record PublicUpcomingEventsBlockDataResponse(IReadOnlyList<PublicContentListBlockItemResponse> Items);
