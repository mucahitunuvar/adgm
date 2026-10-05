using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1: a versioned legal text (privacy notice, explicit consent, cookie policy, terms...),
// entirely panel-managed - never seeded. RowVersion is the same application-managed optimistic-
// concurrency token every other Website aggregate uses; it also guards the Draft version's own edits,
// since LegalDocumentVersion itself carries no concurrency token (mirrors Slide under Slider).
public sealed class LegalDocument : AggregateRoot
{
    private readonly List<LegalDocumentTranslation> _translations = [];
    private readonly List<LegalDocumentVersion> _versions = [];

    public LegalDocumentKey Key { get; private set; } = null!;

    public LegalDocumentKind Kind { get; private set; }

    public IReadOnlyList<LegalDocumentTranslation> Translations => _translations.AsReadOnly();

    public IReadOnlyList<LegalDocumentVersion> Versions => _versions.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    // §12.1 "Hiç yayınlanmış sürümü olmayan doküman silinebilir": once any version has ever left Draft,
    // the document stays undeletable forever - even if every version is later Superseded, since there
    // is no "unpublish" for a version and a past consumer (an accepted form submission) may still cite
    // it by version number.
    public bool HasEverBeenPublished => _versions.Any(v => v.Status != LegalDocumentVersionStatus.Draft);

    public bool HasDraft => _versions.Any(v => v.Status == LegalDocumentVersionStatus.Draft);

    private LegalDocument(Guid id, LegalDocumentKey key, LegalDocumentKind kind, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        Key = key;
        Kind = kind;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private LegalDocument()
    {
    }

    public static Result<LegalDocument> Create(
        string? key,
        LegalDocumentKind kind,
        LanguageCode defaultLanguageCode,
        string? defaultLanguageTitle,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var keyResult = LegalDocumentKey.Create(key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<LegalDocument>(keyResult.Error);
        }

        var translationResult = LegalDocumentTranslation.Create(defaultLanguageCode, defaultLanguageTitle);
        if (translationResult.IsFailure)
        {
            return Result.Failure<LegalDocument>(translationResult.Error);
        }

        var document = new LegalDocument(Guid.NewGuid(), keyResult.Value, kind, createdByUserId, createdAtUtc);
        document._translations.Add(translationResult.Value);

        return Result.Success(document);
    }

    // Upserts the Title translation for languageCode - one call per language the caller wants to set.
    public Result SetTranslation(LanguageCode languageCode, string? title, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            var updateResult = existing.Update(title);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = LegalDocumentTranslation.Create(languageCode, title);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // §12.1 "Aynı anda en fazla bir Draft sürüm vardır; yeni sürüm taslağı istenirse mevcut yayındaki
    // sürümün metinleri kopyalanarak oluşturulur." now resolves which version currently qualifies as
    // effective (LegalDocumentEffectiveVersionResolver) - not simply the latest Published/Superseded
    // row, since a future-dated one is not yet effective and must not be copied from.
    public Result CreateDraft(string? changeSummary, Guid updatedByUserId, DateTime now)
    {
        if (HasDraft)
        {
            return Result.Failure(Error.Conflict("LegalDocument.DraftAlreadyExists", "This document already has a draft version."));
        }

        var nextVersionNumber = _versions.Count == 0 ? 1 : _versions.Max(v => v.VersionNumber) + 1;
        var effectiveVersion = LegalDocumentEffectiveVersionResolver.Resolve(_versions, now);

        var initialTranslations = new List<LegalDocumentVersionTranslation>();
        if (effectiveVersion is not null)
        {
            foreach (var translation in effectiveVersion.Translations)
            {
                var copyResult = LegalDocumentVersionTranslation.Create(translation.LanguageCode, translation.Body);
                if (copyResult.IsFailure)
                {
                    return copyResult;
                }

                initialTranslations.Add(copyResult.Value);
            }
        }

        var versionResult = LegalDocumentVersion.CreateDraft(nextVersionNumber, changeSummary, initialTranslations);
        if (versionResult.IsFailure)
        {
            return versionResult;
        }

        _versions.Add(versionResult.Value);
        Touch(updatedByUserId, now);

        return Result.Success();
    }

    public Result UpdateDraftBody(LanguageCode languageCode, string? body, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var draft = _versions.FirstOrDefault(v => v.Status == LegalDocumentVersionStatus.Draft);
        if (draft is null)
        {
            return Result.Failure(Error.NotFound("LegalDocument.DraftNotFound", "This document has no draft version."));
        }

        var result = draft.SetBody(languageCode, body);
        if (result.IsFailure)
        {
            return result;
        }

        Touch(updatedByUserId, updatedAtUtc);
        return Result.Success();
    }

    public Result PublishDraft(LanguageCode defaultLanguageCode, DateTime? effectiveAtUtc, Guid publishedByUserId, DateTime now)
    {
        var draft = _versions.FirstOrDefault(v => v.Status == LegalDocumentVersionStatus.Draft);
        if (draft is null)
        {
            return Result.Failure(Error.NotFound("LegalDocument.DraftNotFound", "This document has no draft version."));
        }

        var currentlyPublished = _versions.FirstOrDefault(v => v.Status == LegalDocumentVersionStatus.Published);

        var publishResult = draft.Publish(defaultLanguageCode, effectiveAtUtc, publishedByUserId, now);
        if (publishResult.IsFailure)
        {
            return publishResult;
        }

        currentlyPublished?.Supersede();

        Touch(publishedByUserId, now);
        return Result.Success();
    }

    public Result DeleteDraft(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var draft = _versions.FirstOrDefault(v => v.Status == LegalDocumentVersionStatus.Draft);
        if (draft is null)
        {
            return Result.Failure(Error.NotFound("LegalDocument.DraftNotFound", "This document has no draft version."));
        }

        _versions.Remove(draft);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
