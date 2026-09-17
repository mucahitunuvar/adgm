using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// LanguageId/LanguageLevelId reference ReferenceData's Language (seed) and LanguageLevel
// (admin-managed) lookups, validated at write time by the Application layer (ADR-016), not here.
public sealed class CandidateLanguage : Entity
{
    public Guid CandidateCvContentId { get; private set; }

    public Guid LanguageId { get; private set; }

    public Guid LanguageLevelId { get; private set; }

    public bool IsNativeLanguage { get; private set; }

    private CandidateLanguage(Guid id, Guid candidateCvContentId, Guid languageId, Guid languageLevelId, bool isNativeLanguage)
        : base(id)
    {
        CandidateCvContentId = candidateCvContentId;
        LanguageId = languageId;
        LanguageLevelId = languageLevelId;
        IsNativeLanguage = isNativeLanguage;
    }

    internal static CandidateLanguage Create(
        Guid candidateCvContentId, Guid languageId, Guid languageLevelId, bool isNativeLanguage) =>
        new(Guid.NewGuid(), candidateCvContentId, languageId, languageLevelId, isNativeLanguage);

    public void Update(Guid languageId, Guid languageLevelId, bool isNativeLanguage)
    {
        LanguageId = languageId;
        LanguageLevelId = languageLevelId;
        IsNativeLanguage = isNativeLanguage;
    }
}
