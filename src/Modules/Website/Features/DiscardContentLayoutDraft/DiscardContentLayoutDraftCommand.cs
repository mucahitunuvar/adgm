using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DiscardContentLayoutDraft;

public sealed record DiscardContentLayoutDraftCommand(Guid ContentItemId, byte[] RowVersion) : IRequest<Result>;
