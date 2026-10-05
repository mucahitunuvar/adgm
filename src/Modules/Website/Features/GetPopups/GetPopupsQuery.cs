using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPopups;

public sealed record GetPopupsQuery(string? DisplayMode, bool? IsActive)
    : PagedRequest, IRequest<Result<PagedResult<PopupSummaryResponse>>>;
