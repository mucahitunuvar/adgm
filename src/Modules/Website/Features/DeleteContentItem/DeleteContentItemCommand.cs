using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentItem;

public sealed record DeleteContentItemCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
