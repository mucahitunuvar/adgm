using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employer.Domain;

// LanguageId/LanguageLevelId, ReferenceData'nın Language/LanguageLevel lookup'larına referans verir,
// write-time'da doğrulanmaz (ADR-016) - CandidateLanguage.cs ile aynı desen.
public sealed class JobLanguageRequirement : Entity
{
    public Guid JobId { get; private set; }

    public Guid LanguageId { get; private set; }

    public Guid LanguageLevelId { get; private set; }

    private JobLanguageRequirement(Guid id, Guid jobId, Guid languageId, Guid languageLevelId)
        : base(id)
    {
        JobId = jobId;
        LanguageId = languageId;
        LanguageLevelId = languageLevelId;
    }

    internal static JobLanguageRequirement Create(Guid jobId, Guid languageId, Guid languageLevelId) =>
        new(Guid.NewGuid(), jobId, languageId, languageLevelId);
}
