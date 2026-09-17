using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Detail aggregate (ADR-017 / Candidate module design ADR): the collection-heavy, sık-güncellenen
// CV content, kept separate from the CandidateCv header aggregate - see that ADR's rationale.
public sealed class CandidateCvContent : AggregateRoot
{
    private readonly List<Experience> _experiences = [];
    private readonly List<Education> _educations = [];
    private readonly List<CandidateLanguage> _languages = [];
    private readonly List<Certificate> _certificates = [];
    private readonly List<CandidateReference> _references = [];

    public Guid CandidateCvId { get; private set; }

    public string? Summary { get; private set; }

    public IReadOnlyCollection<Experience> Experiences => _experiences.AsReadOnly();

    public IReadOnlyCollection<Education> Educations => _educations.AsReadOnly();

    public string? ComputerSkills { get; private set; }

    public IReadOnlyCollection<CandidateLanguage> Languages => _languages.AsReadOnly();

    public IReadOnlyCollection<Certificate> Certificates => _certificates.AsReadOnly();

    public IReadOnlyCollection<CandidateReference> References => _references.AsReadOnly();

    public string? Hobbies { get; private set; }

    public FileAttachment? CvFile { get; private set; }

    private CandidateCvContent(Guid id, Guid candidateCvId)
        : base(id)
    {
        CandidateCvId = candidateCvId;
    }

    // RegisterCandidateCommand (ADR-017 Decision 2) her zaman boş bir CandidateCvContent oluşturur.
    public static CandidateCvContent Create(Guid candidateCvId) =>
        new(Guid.NewGuid(), candidateCvId);

    public void UpdateSummary(string? summary)
    {
        Summary = summary;

        RaiseContentUpdatedEvent();
    }

    public void UpdateComputerSkills(string? computerSkills)
    {
        ComputerSkills = computerSkills;

        RaiseContentUpdatedEvent();
    }

    public void UpdateHobbies(string? hobbies)
    {
        Hobbies = hobbies;

        RaiseContentUpdatedEvent();
    }

    public void SetCvFile(FileAttachment? cvFile)
    {
        CvFile = cvFile;

        RaiseContentUpdatedEvent();
    }

    public Experience AddExperience(string companyName, DateOnly startDate)
    {
        var experience = Experience.Create(Id, companyName, startDate);
        _experiences.Add(experience);

        RaiseContentUpdatedEvent();

        return experience;
    }

    public void RemoveExperience(Guid experienceId)
    {
        var experience = _experiences.FirstOrDefault(e => e.Id == experienceId);

        if (experience is not null)
        {
            _experiences.Remove(experience);
            RaiseContentUpdatedEvent();
        }
    }

    public Education AddEducation(Guid educationLevelId, DateOnly startDate)
    {
        var education = Education.Create(Id, educationLevelId, startDate);
        _educations.Add(education);

        RaiseContentUpdatedEvent();

        return education;
    }

    public void RemoveEducation(Guid educationId)
    {
        var education = _educations.FirstOrDefault(e => e.Id == educationId);

        if (education is not null)
        {
            _educations.Remove(education);
            RaiseContentUpdatedEvent();
        }
    }

    public CandidateLanguage AddLanguage(Guid languageId, Guid languageLevelId, bool isNativeLanguage)
    {
        var language = CandidateLanguage.Create(Id, languageId, languageLevelId, isNativeLanguage);
        _languages.Add(language);

        RaiseContentUpdatedEvent();

        return language;
    }

    public void RemoveLanguage(Guid candidateLanguageId)
    {
        var language = _languages.FirstOrDefault(l => l.Id == candidateLanguageId);

        if (language is not null)
        {
            _languages.Remove(language);
            RaiseContentUpdatedEvent();
        }
    }

    public Certificate AddCertificate(string name, string issuingInstitution)
    {
        var certificate = Certificate.Create(Id, name, issuingInstitution);
        _certificates.Add(certificate);

        RaiseContentUpdatedEvent();

        return certificate;
    }

    public void RemoveCertificate(Guid certificateId)
    {
        var certificate = _certificates.FirstOrDefault(c => c.Id == certificateId);

        if (certificate is not null)
        {
            _certificates.Remove(certificate);
            RaiseContentUpdatedEvent();
        }
    }

    public CandidateReference AddReference(
        Guid referenceTypeId, Guid referenceLanguageId, string firstName, string lastName)
    {
        var reference = CandidateReference.Create(Id, referenceTypeId, referenceLanguageId, firstName, lastName);
        _references.Add(reference);

        RaiseContentUpdatedEvent();

        return reference;
    }

    public void RemoveReference(Guid candidateReferenceId)
    {
        var reference = _references.FirstOrDefault(r => r.Id == candidateReferenceId);

        if (reference is not null)
        {
            _references.Remove(reference);
            RaiseContentUpdatedEvent();
        }
    }

    private void RaiseContentUpdatedEvent()
    {
        RaiseDomainEvent(new CandidateCvContentUpdatedDomainEvent(Id, CandidateCvId));
    }
}
