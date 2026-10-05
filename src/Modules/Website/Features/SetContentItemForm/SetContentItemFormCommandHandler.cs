using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemForm;

// ADR-024 §12.2 (Faz 3 Görev 3): "yalnızca SupportsForm türlerde atanabilir" - the content type's
// feature flag, not the form's own IsActive, gates assignment (an admin may link a form before
// activating it, mirroring how a cover image can be set before a content item is published).
public sealed class SetContentItemFormCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IFormDefinitionRepository formDefinitionRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetContentItemFormCommand, Result>
{
    public async Task<Result> Handle(SetContentItemFormCommand request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure(Error.NotFound("ContentItem.NotFound", $"Content item '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(contentItem.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.ConcurrencyConflict", "The content item was changed by someone else. Reload and try again."));
        }

        var trashCheck = ContentItemTrashGuard.EnsureEditable(contentItem);
        if (trashCheck.IsFailure)
        {
            return trashCheck;
        }

        if (request.FormDefinitionId is not null)
        {
            var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
            if (contentType is null || !contentType.SupportsForm)
            {
                return Result.Failure(Error.Validation(
                    "ContentItem.ContentTypeDoesNotSupportForm", "This content item's content type does not support a form."));
            }

            var formDefinition = await formDefinitionRepository.GetByIdAsync(request.FormDefinitionId.Value, cancellationToken);
            if (formDefinition is null)
            {
                return Result.Failure(Error.NotFound(
                    "FormDefinition.NotFound", $"Form '{request.FormDefinitionId}' could not be found."));
            }
        }

        contentItem.SetFormDefinition(request.FormDefinitionId, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
