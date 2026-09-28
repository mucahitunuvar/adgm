namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15. ContentItem: resolved to that item's CURRENT FullPath at resolution time, never a
// stored path - this is what makes an automatic redirect chain-free even if the same item's path
// changes more than once (Faz 1a Görev 4). Path: an admin-typed literal target (Görev 5).
public enum RedirectTargetKind
{
    ContentItem,
    Path,
}
