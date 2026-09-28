using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ArchiveContentItem;

public sealed record ArchiveContentItemCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
