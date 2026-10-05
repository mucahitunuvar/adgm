using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;

public sealed record UpdateLegalDocumentDraftBodyCommand(
    Guid Id, string LanguageCode, byte[] RowVersion, string? Body) : IRequest<Result>;
