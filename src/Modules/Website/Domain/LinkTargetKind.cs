namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 1 master prompt §1.1. None is LinkTarget's own "no link" sentinel (LinkTarget.CreateEmpty)
// rather than a nullable owned type - the same choice SeoMetadata already made for an optional VO.
public enum LinkTargetKind
{
    None = 0,
    Content = 1,
    ContentTypeListing = 2,
    InternalPath = 3,
    ExternalUrl = 4,
}
