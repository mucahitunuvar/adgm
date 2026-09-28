using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UnarchiveContentItem;

public sealed record UnarchiveContentItemCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
