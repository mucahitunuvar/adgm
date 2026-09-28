using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItems;

public sealed class GetContentItemsQueryHandler(IContentItemRepository contentItemRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetContentItemsQuery, Result<PagedResult<ContentItemSummaryResponse>>>
{
    public async Task<Result<PagedResult<ContentItemSummaryResponse>>> Handle(GetContentItemsQuery request, CancellationToken cancellationToken)
    {
        ContentItemStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<ContentItemStatus>(request.Status, ignoreCase: true, out var parsedStatus))
            {
                return Result.Failure<PagedResult<ContentItemSummaryResponse>>(Error.Validation(
                    "ContentItem.InvalidStatusFilter", $"'{request.Status}' is not a recognized content item status."));
            }

            status = parsedStatus;
        }

        LanguageCode languageCode;
        var requireLanguage = !string.IsNullOrWhiteSpace(request.LanguageCode);
        if (requireLanguage)
        {
            var languageCodeResult = LanguageCode.Create(request.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<PagedResult<ContentItemSummaryResponse>>(languageCodeResult.Error);
            }

            languageCode = languageCodeResult.Value;
        }
        else
        {
            var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
            if (defaultLanguage is null)
            {
                return Result.Failure<PagedResult<ContentItemSummaryResponse>>(
                    Error.Failure("ContentItem.NoDefaultLanguage", "No default site language is configured."));
            }

            languageCode = defaultLanguage.Code;
        }

        var paged = await contentItemRepository.SearchAsync(
            request.ContentTypeId, status, languageCode, requireLanguage, request.Search, request.IsFeatured, request.ParentId,
            request, cancellationToken);

        var items = paged.Items
            .Select(x => new ContentItemSummaryResponse(
                x.Id, x.ContentTypeId, x.ParentId, x.Title, x.Status, x.SortOrder, x.IsFeatured, x.PublishAtUtc, x.UnpublishAtUtc, x.RowVersion))
            .ToList();

        return Result.Success(new PagedResult<ContentItemSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
