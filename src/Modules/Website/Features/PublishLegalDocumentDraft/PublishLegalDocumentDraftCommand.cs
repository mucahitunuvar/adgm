using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;

public sealed record PublishLegalDocumentDraftCommand(Guid Id, byte[] RowVersion, DateTime? EffectiveAtUtc) : IRequest<Result>;
