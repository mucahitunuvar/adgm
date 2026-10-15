using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Parsing;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItems;

public sealed class GetContentItemsQueryHandler(
    IContentItemRepository contentItemRepository, IContentCategoryRepository contentCategoryRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetContentItemsQuery, Result<PagedResult<ContentItemSummaryResponse>>>
{
    public async Task<Result<PagedResult<ContentItemSummaryResponse>>> Handle(GetContentItemsQuery request, CancellationToken cancellationToken)
    {
        ContentItemStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!EnumParsing.TryParseDefined<ContentItemStatus>(request.Status, out var parsedStatus))
            {
                return Result.Failure<PagedResult<ContentItemSummaryResponse>>(Error.Validation(
                    "ContentItem.InvalidStatusFilter", $"'{request.Status}' is not a recognized content item status."));
            }

            status = parsedStatus;
        }

        ContentItemSortField? orderBy = null;
        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            if (!EnumParsing.TryParseDefined<ContentItemSortField>(request.OrderBy, out var parsedOrderBy))
            {
                return Result.Failure<PagedResult<ContentItemSummaryResponse>>(Error.Validation(
                    "ContentItem.InvalidOrderBy", $"'{request.OrderBy}' is not a recognized sort field."));
            }

            orderBy = parsedOrderBy;
        }

        // Validated even when OrderBy is absent (a garbage Direction value is still a client error),
        // though it only affects sorting once OrderBy is actually given.
        var direction = ContentItemSortDirection.Asc;
        if (!string.IsNullOrWhiteSpace(request.Direction))
        {
            if (!EnumParsing.TryParseDefined<ContentItemSortDirection>(request.Direction, out var parsedDirection))
            {
                return Result.Failure<PagedResult<ContentItemSummaryResponse>>(Error.Validation(
                    "ContentItem.InvalidSortDirection", $"'{request.Direction}' is not a recognized sort direction."));
            }

            direction = parsedDirection;
        }

        if (request.PublishedFromUtc is not null && request.PublishedToUtc is not null && request.PublishedFromUtc > request.PublishedToUtc)
        {
            return Result.Failure<PagedResult<ContentItemSummaryResponse>>(Error.Validation(
                "ContentItem.InvalidPublishedDateRange", "PublishedFromUtc cannot be after PublishedToUtc."));
        }

        if (request.CreatedFromUtc is not null && request.CreatedToUtc is not null && request.CreatedFromUtc > request.CreatedToUtc)
        {
            return Result.Failure<PagedResult<ContentItemSummaryResponse>>(Error.Validation(
                "ContentItem.InvalidCreatedDateRange", "CreatedFromUtc cannot be after CreatedToUtc."));
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

        // ADR-024 §4.1: categories are at most 2 levels deep, so "self + direct children" is the
        // whole subtree - same rule GetPublicContentsQueryHandler applies to the public list's own
        // category filter. A categoryId that matches no category narrows to an empty id list (empty
        // result), not an error - no other filter here (contentTypeId, parentId) validates id
        // existence either.
        IReadOnlyList<Guid>? categoryIds = null;
        if (request.CategoryId is not null)
        {
            categoryIds = await contentCategoryRepository.GetSelfAndChildrenIdsAsync(request.CategoryId.Value, cancellationToken);
        }

        var paged = await contentItemRepository.SearchAsync(
            request.ContentTypeId, status, languageCode, requireLanguage, request.Search, request.IsFeatured, request.ParentId,
            categoryIds, request.TagId, request.PublishedFromUtc, request.PublishedToUtc, request.CreatedFromUtc, request.CreatedToUtc,
            orderBy, direction, request, cancellationToken);

        var items = paged.Items
            .Select(x => new ContentItemSummaryResponse(
                x.Id, x.ContentTypeId, x.ParentId, x.Title, x.Status, x.SortOrder, x.IsFeatured, x.PublishAtUtc, x.UnpublishAtUtc, x.RowVersion))
            .ToList();

        return Result.Success(new PagedResult<ContentItemSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
