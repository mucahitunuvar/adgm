using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteVideo;

public sealed record DeleteVideoCommand(Guid Id) : IRequest<Result>;
