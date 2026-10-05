using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinition;

public sealed class UpdateFormDefinitionCommandHandler(
    IFormDefinitionRepository formDefinitionRepository,
    ILegalDocumentRepository legalDocumentRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateFormDefinitionCommand, Result>
{
    public async Task<Result> Handle(UpdateFormDefinitionCommand request, CancellationToken cancellationToken)
    {
        var form = await formDefinitionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (form is null)
        {
            return Result.Failure(Error.NotFound("FormDefinition.NotFound", $"Form '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(form.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "FormDefinition.ConcurrencyConflict", "The form was changed by someone else. Reload and try again."));
        }

        var privacyNoticeResult = await FormDefinitionLegalReferenceGuard.ResolvePrivacyNoticeKeyAsync(
            request.PrivacyNoticeKey, legalDocumentRepository, cancellationToken);
        if (privacyNoticeResult.IsFailure)
        {
            return privacyNoticeResult;
        }

        var explicitConsentsResult = await FormDefinitionLegalReferenceGuard.ResolveExplicitConsentsAsync(
            request.ExplicitConsents.Select(c => (c.LegalDocumentKey, c.IsRequired)).ToList(), legalDocumentRepository, cancellationToken);
        if (explicitConsentsResult.IsFailure)
        {
            return explicitConsentsResult;
        }

        var retentionDays = request.RetentionDays ?? form.RetentionDays;

        var updateResult = form.Update(
            retentionDays, request.NotificationEmails, privacyNoticeResult.Value, explicitConsentsResult.Value,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
