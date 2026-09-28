using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetRedirects;

public sealed record GetRedirectsQuery(string? LanguageCode, bool? IsAutomatic, string? Search)
    : PagedRequest, IRequest<Result<PagedResult<RedirectResponse>>>;
