using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMediaAssets;

public sealed record GetMediaAssetsQuery(string? Kind, string? Folder, string? Search, bool? MissingAltText)
    : PagedRequest, IRequest<Result<PagedResult<MediaAssetSummaryResponse>>>;
