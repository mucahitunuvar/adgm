using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocumentVersion;

// ADR-024 §12.1 "geçmiş bir sürümü gösterir (bir kişinin onayladığı metni sonradan göstermek için)":
// unlike GetPublicLegalDocumentQuery, this never resolves "the effective version" - it returns exactly
// the requested VersionNumber, as long as it has left Draft (a Draft is never shown publicly) and has
// a translation in the requested language. Content is immutable once published, so the cached entry
// needs no transition-aware TTL shortening - the default TTL is enough.
public sealed class GetPublicLegalDocumentVersionQueryHandler(
    ILegalDocumentRepository legalDocumentRepository, ISiteLanguageRepository siteLanguageRepository, ICacheService cacheService)
    : IRequestHandler<GetPublicLegalDocumentVersionQuery, Result<PublicLegalDocumentVersionResponse>>
{
    private static readonly Error NotFoundError =
        Error.NotFound("LegalDocument.VersionNotFound", "This legal document version could not be found.");

    public async Task<Result<PublicLegalDocumentVersionResponse>> Handle(
        GetPublicLegalDocumentVersionQuery request, CancellationToken cancellationToken)
    {
        var keyResult = LegalDocumentKey.Create(request.Key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<PublicLegalDocumentVersionResponse>(NotFoundError);
        }

        var document = await legalDocumentRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
        if (document is null)
        {
            return Result.Failure<PublicLegalDocumentVersionResponse>(NotFoundError);
        }

        var version = document.Versions.FirstOrDefault(
            v => v.VersionNumber == request.VersionNumber && v.Status != LegalDocumentVersionStatus.Draft);
        if (version is null)
        {
            return Result.Failure<PublicLegalDocumentVersionResponse>(NotFoundError);
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var bodyTranslation = version.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
        if (bodyTranslation is null)
        {
            return Result.Failure<PublicLegalDocumentVersionResponse>(NotFoundError);
        }

        var titleTranslation = document.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
        var title = titleTranslation?.Title ?? document.Key.Value;
        var versionNumber = version.VersionNumber;
        var effectiveAtUtc = version.EffectiveAtUtc!.Value;
        var body = bodyTranslation.Body;

        var cacheKey = WebsiteCacheKeys.PublicLegalDocumentVersion(document.Key.Value, version.VersionNumber, resolvedLanguage.Code.Value);
        var response = await cacheService.GetOrCreateAsync(
            cacheKey,
            _ => Task.FromResult(new PublicLegalDocumentVersionResponse(title, body, versionNumber, effectiveAtUtc)),
            ContentCacheTtlCalculator.DefaultTtl,
            cancellationToken);

        return Result.Success(response);
    }
}
