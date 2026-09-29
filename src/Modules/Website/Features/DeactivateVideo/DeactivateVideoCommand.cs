using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateVideo;

public sealed record DeactivateVideoCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
