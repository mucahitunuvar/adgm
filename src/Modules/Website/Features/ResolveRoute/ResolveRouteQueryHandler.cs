using GenclikMerkezi.Modules.Website.Application.RouteResolution;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ResolveRoute;

public sealed class ResolveRouteQueryHandler(RouteResolutionService routeResolutionService)
    : IRequestHandler<ResolveRouteQuery, Result<RouteResolutionResponse>>
{
    public async Task<Result<RouteResolutionResponse>> Handle(ResolveRouteQuery request, CancellationToken cancellationToken)
    {
        var outcome = await routeResolutionService.ResolveAsync(request.Path, cancellationToken);

        var alternates = outcome.Alternates?.Select(a => new RouteAlternateResponse(a.LanguageCode, a.Path)).ToList();
        var seo = outcome.Seo is null
            ? null
            : new RouteResolutionSeoResponse(
                outcome.Seo.MetaTitle, outcome.Seo.MetaDescription, outcome.Seo.MetaKeywords, outcome.Seo.OgTitle,
                outcome.Seo.OgDescription, outcome.Seo.OgImageMediaId, outcome.Seo.CanonicalUrl, outcome.Seo.NoIndex);

        var response = new RouteResolutionResponse(
            outcome.Kind.ToString(), outcome.LanguageCode, outcome.ContentItemId, outcome.ContentTypeKey, outcome.ListTemplate,
            outcome.DetailTemplate, outcome.Name, seo, alternates, outcome.Location, outcome.StatusCode);

        return Result.Success(response);
    }
}
