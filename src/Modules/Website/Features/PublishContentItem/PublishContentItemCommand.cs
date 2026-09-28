using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.PublishContentItem;

public sealed record PublishContentItemCommand(Guid Id, byte[] RowVersion, DateTime? PublishAtUtc, DateTime? UnpublishAtUtc) : IRequest<Result>;
