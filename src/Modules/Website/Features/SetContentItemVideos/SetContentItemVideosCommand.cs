using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemVideos;

public sealed record SetContentItemVideosCommand(Guid Id, byte[] RowVersion, IReadOnlyList<Guid> VideoIds) : IRequest<Result>;
