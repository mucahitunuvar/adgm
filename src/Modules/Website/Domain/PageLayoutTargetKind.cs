namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §8.1 / Faz 2 Görev 4 master prompt §4.3: a PageLayout attaches either to the single Home
// page or to one SupportsBlockLayout ContentItem - never to anything else.
public enum PageLayoutTargetKind
{
    Home,
    Content,
}
