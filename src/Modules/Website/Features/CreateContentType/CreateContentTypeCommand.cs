using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentType;

public sealed record CreateContentTypeCommand(
    string? Key,
    string? ListTemplate,
    string? DetailTemplate,
    string? SortMode,
    int SortOrder,
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
    bool RequiresReview,
    string? DefaultLanguageName,
    string? DefaultLanguageRoutePrefix,
    CreateContentTypeSeoInput Seo,
    string? SchemaKind = null) : IRequest<Result<CreateContentTypeResponse>>;
