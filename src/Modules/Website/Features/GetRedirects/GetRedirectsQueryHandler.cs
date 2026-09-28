using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetRedirects;

public sealed class GetRedirectsQueryHandler(IRedirectRepository redirectRepository)
    : IRequestHandler<GetRedirectsQuery, Result<PagedResult<RedirectResponse>>>
{
    public async Task<Result<PagedResult<RedirectResponse>>> Handle(GetRedirectsQuery request, CancellationToken cancellationToken)
    {
        LanguageCode? languageCode = null;
        if (!string.IsNullOrWhiteSpace(request.LanguageCode))
        {
            var languageCodeResult = LanguageCode.Create(request.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<PagedResult<RedirectResponse>>(languageCodeResult.Error);
            }

            languageCode = languageCodeResult.Value;
        }

        var paged = await redirectRepository.SearchAsync(languageCode, request.IsAutomatic, request.Search, request, cancellationToken);

        var items = paged.Items
            .Select(r => new RedirectResponse(
                r.Id, r.LanguageCode.Value, r.FromPath, r.TargetKind.ToString(), r.TargetContentItemId, r.TargetPath,
                r.StatusCode.ToString(), r.IsAutomatic, r.HitCount, r.LastHitAtUtc, r.CreatedAtUtc))
            .ToList();

        return Result.Success(new PagedResult<RedirectResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
