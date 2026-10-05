using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocument;

// ADR-024 §12.1: the effective (currently in force) version of a legal document, in the requested
// language. 404s whenever there is nothing to show - unknown key, no version effective yet, or no
// translation for that version in the requested language - never a partial/empty body.
public sealed class GetPublicLegalDocumentQueryHandler(
    ILegalDocumentRepository legalDocumentRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICacheService cacheService,
    TimeProvider timeProvider)
    : IRequestHandler<GetPublicLegalDocumentQuery, Result<PublicLegalDocumentResponse>>
{
    private static readonly Error NotFoundError =
        Error.NotFound("LegalDocument.NotFound", "This legal document could not be found.");

    public async Task<Result<PublicLegalDocumentResponse>> Handle(GetPublicLegalDocumentQuery request, CancellationToken cancellationToken)
    {
        var keyResult = LegalDocumentKey.Create(request.Key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<PublicLegalDocumentResponse>(NotFoundError);
        }

        var document = await legalDocumentRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
        if (document is null)
        {
            return Result.Failure<PublicLegalDocumentResponse>(NotFoundError);
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var effectiveVersion = LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, now);
        if (effectiveVersion is null)
        {
            return Result.Failure<PublicLegalDocumentResponse>(NotFoundError);
        }

        var bodyTranslation = effectiveVersion.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
        if (bodyTranslation is null)
        {
            return Result.Failure<PublicLegalDocumentResponse>(NotFoundError);
        }

        var titleTranslation = document.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
        var title = titleTranslation?.Title ?? document.Key.Value;
        var versionNumber = effectiveVersion.VersionNumber;
        var effectiveAtUtc = effectiveVersion.EffectiveAtUtc!.Value;
        var body = bodyTranslation.Body;

        // §12.1 "TTL bir sonraki EffectiveAtUtc anına göre kısaltılır": a not-yet-effective version
        // (future EffectiveAtUtc) becomes the new effective one the instant its date arrives, so the
        // cached response must expire no later than that.
        var ttl = ContentCacheTtlCalculator.Calculate(now, UpcomingEffectiveDates(document.Versions));
        var cacheKey = WebsiteCacheKeys.PublicLegalDocument(document.Key.Value, resolvedLanguage.Code.Value);

        var response = await cacheService.GetOrCreateAsync(
            cacheKey,
            _ => Task.FromResult(new PublicLegalDocumentResponse(title, body, versionNumber, effectiveAtUtc)),
            ttl,
            cancellationToken);

        return Result.Success(response);
    }

    private static IEnumerable<DateTime?> UpcomingEffectiveDates(IReadOnlyList<LegalDocumentVersion> versions) =>
        versions.Where(v => v.Status != LegalDocumentVersionStatus.Draft).Select(v => v.EffectiveAtUtc);
}
