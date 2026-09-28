using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentItem;

public sealed record CreateContentItemCommand(
    Guid ContentTypeId,
    int SortOrder,
    bool IsFeatured,
    Guid? CoverImageMediaId,
    Guid? DetailImageMediaId,
    string? DefaultLanguageTitle,
    string? DefaultLanguageSlug,
    string? DefaultLanguageSummary,
    string? DefaultLanguageBody,
    CreateContentItemSeoInput Seo) : IRequest<Result<CreateContentItemResponse>>;
