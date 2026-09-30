using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.RestoreContentItem;

public sealed record RestoreContentItemCommand(Guid Id) : IRequest<Result>;
