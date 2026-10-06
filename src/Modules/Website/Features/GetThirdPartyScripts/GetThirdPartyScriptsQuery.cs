using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetThirdPartyScripts;

public sealed record GetThirdPartyScriptsQuery(bool? IsActive, string? Category)
    : PagedRequest, IRequest<Result<PagedResult<ThirdPartyScriptSummaryResponse>>>;
