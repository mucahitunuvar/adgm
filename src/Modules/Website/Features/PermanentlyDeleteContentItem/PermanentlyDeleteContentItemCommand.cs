using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.PermanentlyDeleteContentItem;

public sealed record PermanentlyDeleteContentItemCommand(Guid Id) : IRequest<Result>;
