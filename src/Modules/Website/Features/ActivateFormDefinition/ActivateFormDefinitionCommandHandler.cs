using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ActivateFormDefinition;

// ADR-024 §12.2 "Form ancak bağlı aydınlatma metninin (ve zorunlu açık rızaların) yürürlükte bir sürümü
// varsa aktif edilebilir": this handler is the only place with LegalDocument repository access that
// FormDefinition.Activate needs - it resolves whether the privacy notice and every REQUIRED explicit
// consent currently has an effective version (an optional consent's missing version does not block
// activation) and passes the single bool the domain method expects.
public sealed class ActivateFormDefinitionCommandHandler(
    IFormDefinitionRepository formDefinitionRepository,
    ILegalDocumentRepository legalDocumentRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateFormDefinitionCommand, Result>
{
    public async Task<Result> Handle(ActivateFormDefinitionCommand request, CancellationToken cancellationToken)
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

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var legalRequirementsSatisfied = await HasEffectiveVersionAsync(form.PrivacyNoticeKey, now, cancellationToken);
        foreach (var consent in form.ExplicitConsents.Where(c => c.IsRequired))
        {
            legalRequirementsSatisfied &= await HasEffectiveVersionAsync(consent.LegalDocumentKey, now, cancellationToken);
        }

        var activateResult = form.Activate(legalRequirementsSatisfied, currentUserContext.UserId!.Value, now);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }

    private async Task<bool> HasEffectiveVersionAsync(LegalDocumentKey key, DateTime now, CancellationToken cancellationToken)
    {
        var document = await legalDocumentRepository.GetByKeyAsync(key, cancellationToken);
        return document is not null && LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, now) is not null;
    }
}
