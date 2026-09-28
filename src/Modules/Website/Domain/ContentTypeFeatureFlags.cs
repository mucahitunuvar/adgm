namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1: the 14 feature flags a ContentType carries, bundled as one parameter/return object
// instead of 14 positional bools on ContentType's constructor and Update method - not a ValueObject
// (no validation or equality semantics of its own; ContentType itself owns every cross-flag
// invariant, e.g. EventDateAsc/SupportsEvent and HasListingPage/RoutePrefix in ContentType.Update).
public sealed record ContentTypeFeatureFlags(
    bool SupportsHierarchy,
    bool SupportsCategories,
    bool SupportsTags,
    bool SupportsDetailImage,
    bool SupportsGallery,
    bool SupportsVideos,
    bool SupportsAttachments,
    bool SupportsEvent,
    bool SupportsBlockLayout,
    bool SupportsForm,
    bool SupportsRelatedContent,
    bool HasDetailPage,
    bool HasListingPage,
    bool IsSearchable,
    bool RequiresReview)
{
    public static ContentTypeFeatureFlags None { get; } = new(
        SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
        SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
        SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: false,
        HasListingPage: false, IsSearchable: false, RequiresReview: false);
}
