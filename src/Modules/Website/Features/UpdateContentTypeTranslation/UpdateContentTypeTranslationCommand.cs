using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;

public sealed record UpdateContentTypeTranslationCommand(
    Guid Id,
    string LanguageCode,
    byte[] RowVersion,
    string? Name,
    string? RoutePrefix,
    UpdateContentTypeTranslationSeoInput Seo) : IRequest<Result>;
