using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsNewsletter;

// ADR-024 §14 (Faz 3 Görev 6): a newsletterPrivacyNoticeKey must reference an existing LegalDocument
// of Kind PrivacyNotice - reuses FormDefinitionLegalReferenceGuard instead of duplicating that same
// lookup and Kind check here. An empty/null key simply clears the setting (SubscribeToNewsletter then
// treats the newsletter as not configured, the same as it being disabled).
public sealed class UpdateSiteSettingsNewsletterCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ILegalDocumentRepository legalDocumentRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsNewsletterCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsNewsletterCommand request, CancellationToken cancellationToken)
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
        if (!string.IsNullOrWhiteSpace(request.NewsletterPrivacyNoticeKey))
        {
            var resolveResult = await FormDefinitionLegalReferenceGuard.ResolvePrivacyNoticeKeyAsync(
                request.NewsletterPrivacyNoticeKey, legalDocumentRepository, cancellationToken);
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

        settings.UpdateNewsletterPrivacyNoticeKey(privacyNoticeKey, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
