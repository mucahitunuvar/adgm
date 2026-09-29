using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateTag;

public sealed record UpdateTagCommand(Guid Id, string? Name) : IRequest<Result>;
