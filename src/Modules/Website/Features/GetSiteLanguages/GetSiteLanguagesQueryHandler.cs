using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;

public sealed class GetSiteLanguagesQueryHandler(ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetSiteLanguagesQuery, Result<GetSiteLanguagesResponse>>
{
    public async Task<Result<GetSiteLanguagesResponse>> Handle(GetSiteLanguagesQuery request, CancellationToken cancellationToken)
    {
        var siteLanguages = await siteLanguageRepository.GetAllAsync(cancellationToken);
        var items = siteLanguages.Select(SiteLanguageResponse.FromDomain).ToList();

        return Result.Success(new GetSiteLanguagesResponse(items));
    }
}
