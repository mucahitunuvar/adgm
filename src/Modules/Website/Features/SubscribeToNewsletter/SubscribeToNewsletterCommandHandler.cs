using System.Security.Cryptography;
using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SubscribeToNewsletter;

// ADR-024 §14 (Faz 3 Görev 6). "Kayıt sızdırmaz": every branch below (new row, resend, already-active,
// resubscribe-after-unsubscribe) returns the same Result.Success(), mapped by the endpoint to the same
// 202 - nothing here ever reveals whether an email was already subscribed.
public sealed class SubscribeToNewsletterCommandHandler(
    IPublicSubmissionGuard publicSubmissionGuard,
    ISiteSettingsRepository siteSettingsRepository,
    ILegalDocumentRepository legalDocumentRepository,
    INewsletterSubscriberRepository newsletterSubscriberRepository,
    ISiteLanguageRepository siteLanguageRepository,
    INewsletterConfirmationLinkGenerator confirmationLinkGenerator,
    IWebsiteEmailSender websiteEmailSender,
    IConfiguration configuration,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SubscribeToNewsletterCommand, Result>
{
    public static readonly TimeSpan ConfirmationTokenLifetime = TimeSpan.FromDays(7);

    private static readonly Error NotAvailableError = Error.NotFound(
        "Newsletter.NotAvailable", "Newsletter subscriptions are not available.");

    public async Task<Result> Handle(SubscribeToNewsletterCommand request, CancellationToken cancellationToken)
    {
        var guardResult = await publicSubmissionGuard.VerifyAsync(
            new PublicSubmissionGuardRequest(request.SubmissionToken, request.TurnstileToken, request.Website), request.RemoteIpAddress,
            cancellationToken);
        if (guardResult.IsFailure)
        {
            return guardResult;
        }

        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        var newsletterPrivacyNoticeKey = settings.NewsletterPrivacyNoticeKey;
        if (!settings.NewsletterEnabled || newsletterPrivacyNoticeKey is null)
        {
            return Result.Failure(NotAvailableError);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var document = await legalDocumentRepository.GetByKeyAsync(newsletterPrivacyNoticeKey, cancellationToken);
        var effective = document is null ? null : LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, now);
        if (effective is null)
        {
            return Result.Failure(NotAvailableError);
        }

        if (request.AcceptedPrivacyNoticeVersion != effective.VersionNumber)
        {
            return Result.Failure(Error.Conflict(
                "PublicSubmission.LegalVersionChanged",
                "The legal document you accepted has since changed. Please review and accept the current version."));
        }

        var emailResult = NewsletterSubscriber.NormalizeEmail(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var existing = await newsletterSubscriberRepository.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (existing is null)
        {
            var createResult = NewsletterSubscriber.Create(
                emailResult.Value, resolvedLanguage.Code, newsletterPrivacyNoticeKey, effective.VersionNumber,
                GenerateUnsubscribeToken(), now);
            if (createResult.IsFailure)
            {
                return Result.Failure(createResult.Error);
            }

            var subscriber = createResult.Value;
            subscriber.RecordConfirmationEmailSent(now);
            newsletterSubscriberRepository.Add(subscriber);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await SendConfirmationEmailAsync(subscriber, cancellationToken);

            return Result.Success();
        }

        switch (existing.Status)
        {
            case NewsletterSubscriberStatus.Active:
                return Result.Success();

            case NewsletterSubscriberStatus.PendingConfirmation:
                if (existing.CanSendConfirmationEmail(now))
                {
                    existing.RecordConfirmationEmailSent(now);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    await SendConfirmationEmailAsync(existing, cancellationToken);
                }

                return Result.Success();

            case NewsletterSubscriberStatus.Unsubscribed:
                existing.Resubscribe(newsletterPrivacyNoticeKey, effective.VersionNumber, resolvedLanguage.Code, now);
                existing.RecordConfirmationEmailSent(now);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await SendConfirmationEmailAsync(existing, cancellationToken);

                return Result.Success();

            default:
                return Result.Success();
        }
    }

    // §14 "Her bülten e-postasında kullanılacak ... link adresi yapılandırmadan okunur
    // (Website:PublicSiteBaseUrl); e-postadaki link frontend sayfasına gider, frontend token'ı API'ye
    // iletir" - mirrors SubmitFormSubmissionCommandHandler's own AdminPanelBaseUrl lookup (optional,
    // empty by default).
    private async Task SendConfirmationEmailAsync(NewsletterSubscriber subscriber, CancellationToken cancellationToken)
    {
        var token = confirmationLinkGenerator.GenerateToken(subscriber.Id, ConfirmationTokenLifetime);
        var publicSiteBaseUrl = (configuration["Website:PublicSiteBaseUrl"] ?? string.Empty).TrimEnd('/');
        var confirmUrl = $"{publicSiteBaseUrl}/newsletter/confirm?token={Uri.EscapeDataString(token)}";

        const string subject = "Confirm your newsletter subscription";
        var body = "<p>Please confirm your newsletter subscription by clicking the link below.</p>"
            + $"<p><a href=\"{confirmUrl}\">Confirm subscription</a></p>";

        await websiteEmailSender.SendEmailAsync(subscriber.Email, subject, body, cancellationToken);
    }

    private static string GenerateUnsubscribeToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
}
