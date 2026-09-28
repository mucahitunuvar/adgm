using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;

public sealed record UpdateContentItemTranslationCommand(
    Guid Id,
    string LanguageCode,
    byte[] RowVersion,
    string? Title,
    string? Slug,
    string? Summary,
    string? Body,
    UpdateContentItemTranslationSeoInput Seo) : IRequest<Result>;
