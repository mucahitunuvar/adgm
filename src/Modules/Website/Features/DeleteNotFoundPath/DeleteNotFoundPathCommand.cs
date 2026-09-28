using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteNotFoundPath;

public sealed record DeleteNotFoundPathCommand(Guid Id) : IRequest<Result>;
