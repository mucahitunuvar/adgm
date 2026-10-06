namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1. EventDateAsc is only selectable on a SupportsEvent type (ContentType invariant).
// Faz 4 Görev 2: list queries now sort by the linked EventSchedule's StartsAtUtc ascending, with
// items that have no schedule yet sinking to the end (ContentItemRepository.SearchPublicListAsync) -
// the ADR-024 §4 note documenting the old PublishDateDesc-equivalent placeholder behavior is removed
// in Görev 6's doc sync.
public enum ContentTypeSortMode
{
    Manual,
    PublishDateDesc,
    EventDateAsc,
}
