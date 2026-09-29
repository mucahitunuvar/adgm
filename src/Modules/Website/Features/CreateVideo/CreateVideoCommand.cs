using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateVideo;

public sealed record CreateVideoCommand(
    string? YouTubeUrl,
    Guid? CoverImageMediaId,
    int SortOrder,
    string? DefaultLanguageTitle,
    string? DefaultLanguageDescription) : IRequest<Result<CreateVideoResponse>>;
