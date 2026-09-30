using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentTrash;

public sealed class GetContentTrashQueryHandler(IContentItemRepository contentItemRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetContentTrashQuery, Result<PagedResult<ContentItemTrashSummaryResponse>>>
{
    public async Task<Result<PagedResult<ContentItemTrashSummaryResponse>>> Handle(GetContentTrashQuery request, CancellationToken cancellationToken)
    {
        LanguageCode languageCode;
        if (!string.IsNullOrWhiteSpace(request.LanguageCode))
        {
            var languageCodeResult = LanguageCode.Create(request.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<PagedResult<ContentItemTrashSummaryResponse>>(languageCodeResult.Error);
            }

            languageCode = languageCodeResult.Value;
        }
        else
        {
            var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
            if (defaultLanguage is null)
            {
                return Result.Failure<PagedResult<ContentItemTrashSummaryResponse>>(
                    Error.Failure("ContentItem.NoDefaultLanguage", "No default site language is configured."));
            }

            languageCode = defaultLanguage.Code;
        }

        var paged = await contentItemRepository.SearchTrashedAsync(languageCode, request, cancellationToken);

        var items = paged.Items
            .Select(x => new ContentItemTrashSummaryResponse(
                x.Id, x.ContentTypeId, x.Title, x.DeletedAtUtc, x.EligibleForPermanentDeletionAtUtc, x.RowVersion))
            .ToList();

        return Result.Success(new PagedResult<ContentItemTrashSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
