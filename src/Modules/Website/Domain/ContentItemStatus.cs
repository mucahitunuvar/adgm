namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.4 (Faz 1a Görev 3). Allowed transitions (enforced by ContentItem, not here):
//   Draft -> Published (Publish)
//   Unpublished -> Published (Publish, i.e. "republish")
//   Published -> Unpublished (Unpublish)
//   Published -> Archived (Archive)
//   Unpublished -> Archived (Archive)
//   Archived -> Unpublished (Unarchive)
// Draft cannot go anywhere except Published; Archived cannot go directly back to Published.
public enum ContentItemStatus
{
    Draft,
    Published,
    Unpublished,
    Archived,
}
