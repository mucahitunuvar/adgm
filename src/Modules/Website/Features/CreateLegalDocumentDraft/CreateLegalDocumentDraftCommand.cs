using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;

public sealed record CreateLegalDocumentDraftCommand(
    Guid Id, byte[] RowVersion, string? ChangeSummary) : IRequest<Result<CreateLegalDocumentDraftResponse>>;
