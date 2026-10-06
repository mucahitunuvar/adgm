using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetThirdPartyScripts;

public sealed class GetThirdPartyScriptsQueryHandler(IThirdPartyScriptRepository thirdPartyScriptRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetThirdPartyScriptsQuery, Result<PagedResult<ThirdPartyScriptSummaryResponse>>>
{
    public async Task<Result<PagedResult<ThirdPartyScriptSummaryResponse>>> Handle(
        GetThirdPartyScriptsQuery request, CancellationToken cancellationToken)
    {
        ThirdPartyScriptCategory? category = null;
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            if (!Enum.TryParse<ThirdPartyScriptCategory>(request.Category, out var parsedCategory))
            {
                return Result.Failure<PagedResult<ThirdPartyScriptSummaryResponse>>(
                    Error.Validation("ThirdPartyScript.CategoryInvalid", "Category must be one of 'Necessary', 'Analytics', 'Marketing'."));
            }

            category = parsedCategory;
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<PagedResult<ThirdPartyScriptSummaryResponse>>(
                Error.Failure("ThirdPartyScript.NoDefaultLanguage", "No default site language is configured."));
        }

        var paged = await thirdPartyScriptRepository.SearchAsync(request.IsActive, category, request, cancellationToken);

        var items = paged.Items
            .Select(s => new ThirdPartyScriptSummaryResponse(
                s.Id, s.Provider.Kind.ToString(), s.Category.ToString(), s.Placement.ToString(),
                s.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguage.Code)?.Name ?? string.Empty,
                s.SortOrder, s.IsActive, s.RowVersion))
            .ToList();

        return Result.Success(new PagedResult<ThirdPartyScriptSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
