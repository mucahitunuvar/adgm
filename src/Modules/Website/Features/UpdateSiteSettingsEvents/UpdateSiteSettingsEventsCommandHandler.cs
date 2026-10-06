using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsEvents;

// ADR-024 §11.2 (Faz 4 Görev 3): mirrors UpdateSiteSettingsNewsletterCommandHandler exactly - an
// empty/null key clears the setting (CreateEventRegistration then treats registration as not yet
// available, the same 404 pattern SubscribeToNewsletter already uses for its own privacy notice key).
public sealed class UpdateSiteSettingsEventsCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ILegalDocumentRepository legalDocumentRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsEventsCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsEventsCommand request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        var isNew = settings is null;
        settings ??= SiteSettings.CreateDefault();

        if (!isNew && !request.RowVersion.SequenceEqual(settings.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "SiteSettings.ConcurrencyConflict", "Site settings were changed by someone else. Reload and try again."));
        }

        LegalDocumentKey? privacyNoticeKey = null;
        if (!string.IsNullOrWhiteSpace(request.EventPrivacyNoticeKey))
        {
            var resolveResult = await FormDefinitionLegalReferenceGuard.ResolvePrivacyNoticeKeyAsync(
                request.EventPrivacyNoticeKey, legalDocumentRepository, cancellationToken);
            if (resolveResult.IsFailure)
            {
                return Result.Failure(resolveResult.Error);
            }

            privacyNoticeKey = resolveResult.Value;
        }

        if (isNew)
        {
            siteSettingsRepository.Add(settings);
        }

        settings.UpdateEventPrivacyNoticeKey(privacyNoticeKey, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
