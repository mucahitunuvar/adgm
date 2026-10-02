using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DiscardHomeLayoutDraft;

public sealed record DiscardHomeLayoutDraftCommand(byte[] RowVersion) : IRequest<Result>;
