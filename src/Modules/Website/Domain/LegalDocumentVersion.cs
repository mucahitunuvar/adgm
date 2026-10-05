using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1: one version of a LegalDocument's text. VersionNumber is assigned by the owning
// LegalDocument (1-based, strictly increasing, never reused). Only ever mutated through
// LegalDocument - RowVersion/concurrency lives on the aggregate root, not here, the same split
// Slide uses under Slider.
public sealed class LegalDocumentVersion : Entity
{
    public const int MaxChangeSummaryLength = 500;

    private readonly List<LegalDocumentVersionTranslation> _translations = [];

    public int VersionNumber { get; private set; }

    public LegalDocumentVersionStatus Status { get; private set; }

    public DateTime? EffectiveAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public Guid? PublishedByUserId { get; private set; }

    public string? ChangeSummary { get; private set; }

    public IReadOnlyList<LegalDocumentVersionTranslation> Translations => _translations.AsReadOnly();

    private LegalDocumentVersion(Guid id, int versionNumber, string? changeSummary)
        : base(id)
    {
        VersionNumber = versionNumber;
        Status = LegalDocumentVersionStatus.Draft;
        ChangeSummary = changeSummary;
    }

    private LegalDocumentVersion()
    {
    }

    // §12.1 "Yeni sürüm taslağı istenirse mevcut yayındaki sürümün metinleri kopyalanarak oluşturulur":
    // initialTranslations is either empty (the document's very first draft) or a copy of the currently
    // effective version's own translations - the caller (LegalDocument.CreateDraft) decides which.
    public static Result<LegalDocumentVersion> CreateDraft(
        int versionNumber, string? changeSummary, IReadOnlyList<LegalDocumentVersionTranslation> initialTranslations)
    {
        var changeSummaryResult = NormalizeChangeSummary(changeSummary);
        if (changeSummaryResult.IsFailure)
        {
            return Result.Failure<LegalDocumentVersion>(changeSummaryResult.Error);
        }

        var version = new LegalDocumentVersion(Guid.NewGuid(), versionNumber, changeSummaryResult.Value);
        version._translations.AddRange(initialTranslations);

        return Result.Success(version);
    }

    // Upserts this draft's body for languageCode. The Status check is defense-in-depth - the only
    // caller, LegalDocument.UpdateDraftBody, already looks up the Draft version specifically - the
    // same belt-and-suspenders every other aggregate's internal mutators use.
    internal Result SetBody(LanguageCode languageCode, string? body)
    {
        if (Status != LegalDocumentVersionStatus.Draft)
        {
            return Result.Failure(Error.Conflict("LegalDocumentVersion.NotDraft", "Only a draft version's body can be edited."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            return existing.Update(body);
        }

        var createResult = LegalDocumentVersionTranslation.Create(languageCode, body);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        _translations.Add(createResult.Value);

        return Result.Success();
    }

    // §12.1 "Yayınlama: varsayılan dilde gövde dolu olmalı; EffectiveAtUtc verilmezse yayın anıdır."
    internal Result Publish(LanguageCode defaultLanguageCode, DateTime? effectiveAtUtc, Guid publishedByUserId, DateTime now)
    {
        if (Status != LegalDocumentVersionStatus.Draft)
        {
            return Result.Failure(Error.Conflict("LegalDocumentVersion.NotDraft", "Only a draft version can be published."));
        }

        var defaultBody = _translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode)?.Body;
        if (string.IsNullOrWhiteSpace(defaultBody))
        {
            return Result.Failure(Error.Validation(
                "LegalDocumentVersion.DefaultLanguageBodyRequired", "The default language's body must be filled in before publishing."));
        }

        Status = LegalDocumentVersionStatus.Published;
        EffectiveAtUtc = effectiveAtUtc ?? now;
        PublishedAtUtc = now;
        PublishedByUserId = publishedByUserId;

        return Result.Success();
    }

    internal void Supersede() => Status = LegalDocumentVersionStatus.Superseded;

    private static Result<string?> NormalizeChangeSummary(string? changeSummary)
    {
        var trimmed = changeSummary?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Success<string?>(null);
        }

        if (trimmed.Length > MaxChangeSummaryLength)
        {
            return Result.Failure<string?>(Error.Validation(
                "LegalDocumentVersion.ChangeSummaryTooLong", $"Change summary must be at most {MaxChangeSummaryLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }
}
