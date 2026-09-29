using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentCategory;

public sealed record CreateContentCategoryCommand(
    Guid ContentTypeId,
    Guid? ParentId,
    int SortOrder,
    string? DefaultLanguageName,
    string? DefaultLanguageSlug,
    CreateContentCategorySeoInput Seo) : IRequest<Result<CreateContentCategoryResponse>>;
