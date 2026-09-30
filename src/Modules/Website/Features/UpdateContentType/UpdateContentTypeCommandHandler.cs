using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentType;

public sealed class UpdateContentTypeCommandHandler(
    IContentTypeRepository contentTypeRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateContentTypeCommand, Result>
{
    public async Task<Result> Handle(UpdateContentTypeCommand request, CancellationToken cancellationToken)
    {
        var contentType = await contentTypeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure(Error.NotFound("ContentType.NotFound", $"Content type '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(contentType.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ContentType.ConcurrencyConflict", "The content type was changed by someone else. Reload and try again."));
        }

        if (!Enum.TryParse<ContentTypeSortMode>(request.SortMode, ignoreCase: true, out var sortMode))
        {
            return Result.Failure(Error.Validation(
                "ContentType.InvalidSortMode", $"'{request.SortMode}' is not a recognized sort mode."));
        }

        var flags = new ContentTypeFeatureFlags(
            request.SupportsHierarchy, request.SupportsCategories, request.SupportsTags, request.SupportsDetailImage,
            request.SupportsGallery, request.SupportsVideos, request.SupportsAttachments, request.SupportsEvent,
            request.SupportsBlockLayout, request.SupportsForm, request.SupportsRelatedContent, request.HasDetailPage,
            request.HasListingPage, request.IsSearchable, request.RequiresReview);

        // ADR-024 §4.1's "a flag cannot be turned off while the type has content" rule (SupportsHierarchy
        // needs no content with a parent, SupportsDetailImage needs no content with a detail image) is
        // not enforced here: it depends on ContentItem, which does not exist until Görev 3. Nothing can
        // violate it yet, since no content exists - Görev 3 adds the real check against
        // IContentItemRepository once that repository exists.
        var updateResult = contentType.Update(
            request.ListTemplate, request.DetailTemplate, sortMode, request.SortOrder, flags,
            currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
