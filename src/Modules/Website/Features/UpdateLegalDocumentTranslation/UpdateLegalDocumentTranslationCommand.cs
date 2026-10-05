using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentTranslation;

public sealed record UpdateLegalDocumentTranslationCommand(
    Guid Id, string LanguageCode, byte[] RowVersion, string? Title) : IRequest<Result>;
