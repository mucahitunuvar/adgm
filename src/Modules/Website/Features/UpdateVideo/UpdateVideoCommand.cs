using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateVideo;

public sealed record UpdateVideoCommand(
    Guid Id, byte[] RowVersion, string? YouTubeUrl, Guid? CoverImageMediaId, int SortOrder) : IRequest<Result>;
