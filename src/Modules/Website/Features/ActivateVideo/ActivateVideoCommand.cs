using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ActivateVideo;

public sealed record ActivateVideoCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
