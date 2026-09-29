using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategoryTranslation;

public sealed record UpdateContentCategoryTranslationCommand(
    Guid TypeId,
    Guid Id,
    string LanguageCode,
    byte[] RowVersion,
    string? Name,
    string? Slug,
    UpdateContentCategoryTranslationSeoInput Seo) : IRequest<Result>;
