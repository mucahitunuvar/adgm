namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 2 master prompt §2: a Slide's own visibility - IsActive plus its publish/unpublish
// window - defined once, the same single-source-of-truth rationale ContentItemVisibility documents,
// so SliderPublicQueryService (and any later caller, e.g. a future earliest-upcoming-transition query)
// never duplicates this two-sided time check by hand.
public static class SlideVisibility
{
    public static bool Evaluate(Slide slide, DateTime now) =>
        slide.IsActive
        && (slide.PublishAtUtc is null || slide.PublishAtUtc <= now)
        && (slide.UnpublishAtUtc is null || slide.UnpublishAtUtc > now);
}
