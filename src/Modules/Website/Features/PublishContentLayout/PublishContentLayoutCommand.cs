using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.PublishContentLayout;

public sealed record PublishContentLayoutCommand(Guid ContentItemId, byte[] RowVersion) : IRequest<Result>;
