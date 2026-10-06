namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: upcoming-events' `data` - visible items from every selected
// content type, merged and capped at Count. Faz 4 Görev 2: for a SupportsEvent type with
// SortMode = EventDateAsc, SearchPublicListAsync now sorts these by the linked EventSchedule's
// StartsAtUtc ascending (see ContentTypeSortMode's remarks) - this block picks that up for free.
public sealed record PublicUpcomingEventsBlockDataResponse(IReadOnlyList<PublicContentListBlockItemResponse> Items);
