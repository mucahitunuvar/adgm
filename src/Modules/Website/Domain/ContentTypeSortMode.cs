namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1. EventDateAsc is only selectable on a SupportsEvent type (ContentType invariant) and,
// until the event calendar ships in Faz 4, list queries apply it identically to PublishDateDesc - a
// stored choice this phase cannot yet act on differently, not a bug (Görev 2 comment on the seed).
public enum ContentTypeSortMode
{
    Manual,
    PublishDateDesc,
    EventDateAsc,
}
