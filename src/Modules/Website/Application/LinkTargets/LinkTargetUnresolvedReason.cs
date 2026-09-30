namespace GenclikMerkezi.Modules.Website.Application.LinkTargets;

// Faz 2 Görev 1 master prompt §1.1/§1.2: why a Content/ContentTypeListing LinkTarget did not resolve
// to an href - the public response only needs "hide it" (Href is null either way), but the admin menu
// editor needs to explain WHY a link is broken (§1.2 "editör kırık linkleri görebilsin").
// InternalPath/ExternalUrl targets never produce anything but None - their own Create already
// validated their one and only shape.
public enum LinkTargetUnresolvedReason
{
    None = 0,
    ContentNotFound,
    ContentNotVisible,
    ContentTypeInactive,
    ContentTypeHasNoDetailPage,
    ContentTranslationMissing,
    ContentTypeNotFound,
    ContentTypeHasNoListingPage,
    ContentTypeTranslationMissing,
}
