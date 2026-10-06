using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateCookieConsentRecord;

// ADR-024 §13 (Faz 3 Görev 7): "banner her ziyaretçiye çıkar, token akışı orantısız olur" - unlike every
// other anonymous write in this module, this one does NOT call IPublicSubmissionGuard. It still only
// records a decision against whatever CookiePolicyKey/version is currently configured and effective -
// the same "resolve effective version, 409 on mismatch" shape SubscribeToNewsletterCommandHandler uses
// for its own privacy notice, reusing the identical error code (§13 implies the same contract Görev 4/6
// already established for a stale legal-document version).
public sealed class CreateCookieConsentRecordCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ILegalDocumentRepository legalDocumentRepository,
    ICookieConsentRecordRepository cookieConsentRecordRepository,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCookieConsentRecordCommand, Result>
{
    private static readonly Error NotAvailableError = Error.NotFound(
        "CookieConsent.NotAvailable", "Cookie consent is not available.");

    public async Task<Result> Handle(CreateCookieConsentRecordCommand request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        var cookiePolicyKey = settings.CookiePolicyKey;
        if (cookiePolicyKey is null)
        {
            return Result.Failure(NotAvailableError);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var document = await legalDocumentRepository.GetByKeyAsync(cookiePolicyKey, cancellationToken);
        var effective = document is null ? null : LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, now);
        if (effective is null)
        {
            return Result.Failure(NotAvailableError);
        }

        if (request.PolicyVersion != effective.VersionNumber)
        {
            return Result.Failure(Error.Conflict(
                "PublicSubmission.LegalVersionChanged", "The legal document you accepted has since changed. Please review and accept the current version."));
        }

        if (!Enum.TryParse<CookieConsentAction>(request.Action, out var action))
        {
            return Result.Failure(Error.Validation(
                "CookieConsent.ActionInvalid", "Action must be one of 'AcceptAll', 'RejectAll', 'Custom'."));
        }

        var categories = new List<ThirdPartyScriptCategory>();
        foreach (var raw in request.Categories ?? [])
        {
            if (!Enum.TryParse<ThirdPartyScriptCategory>(raw, out var category))
            {
                return Result.Failure(Error.Validation(
                    "CookieConsent.CategoryInvalid", "Categories must each be one of 'Necessary', 'Analytics', 'Marketing'."));
            }

            categories.Add(category);
        }

        var recordResult = CookieConsentRecord.Create(request.ConsentId, categories, cookiePolicyKey, request.PolicyVersion, action, now);
        if (recordResult.IsFailure)
        {
            return Result.Failure(recordResult.Error);
        }

        cookieConsentRecordRepository.Add(recordResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
