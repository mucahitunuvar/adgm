using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;

public sealed record UnpublishContentItemCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
