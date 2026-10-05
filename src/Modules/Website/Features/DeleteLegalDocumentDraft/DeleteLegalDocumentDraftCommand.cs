using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteLegalDocumentDraft;

public sealed record DeleteLegalDocumentDraftCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
