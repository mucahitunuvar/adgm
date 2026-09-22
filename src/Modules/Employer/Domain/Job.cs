using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Employer.Domain;

// Aggregate root (ADR-023 §2). CandidateCv'nin aksine (skaler + tek koleksiyon) burada beş gerçek
// child collection var (çoklu seçim tercihleri + dil şartları); Company'nin düz yapısından farklı.
public sealed class Job : AggregateRoot
{
    private readonly List<JobGenderPreference> _genderPreferences = [];
    private readonly List<JobMilitaryStatusPreference> _militaryStatusPreferences = [];
    private readonly List<JobEducationLevelPreference> _educationLevelPreferences = [];
    private readonly List<JobDrivingLicensePreference> _drivingLicensePreferences = [];
    private readonly List<JobLanguageRequirement> _languageRequirements = [];

    public Guid CompanyId { get; private set; }

    public string Title { get; private set; }

    public bool IsForDisabledCandidates { get; private set; }

    public Guid EmploymentTypeId { get; private set; }

    public Guid WorkLocationTypeId { get; private set; }

    public Guid PositionId { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid ProvinceId { get; private set; }

    public string? DescriptionHtml { get; private set; }

    public Guid ExperienceLevelId { get; private set; }

    public IReadOnlyCollection<JobGenderPreference> GenderPreferences => _genderPreferences.AsReadOnly();

    public IReadOnlyCollection<JobMilitaryStatusPreference> MilitaryStatusPreferences => _militaryStatusPreferences.AsReadOnly();

    public IReadOnlyCollection<JobEducationLevelPreference> EducationLevelPreferences => _educationLevelPreferences.AsReadOnly();

    public IReadOnlyCollection<JobDrivingLicensePreference> DrivingLicensePreferences => _drivingLicensePreferences.AsReadOnly();

    public IReadOnlyCollection<JobLanguageRequirement> LanguageRequirements => _languageRequirements.AsReadOnly();

    public JobStatus Status { get; private set; }

    public Guid? ReviewedByAdvisorId { get; private set; }

    public DateTime? ReviewedAtUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public string? RevisionNotes { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public Guid? SuspendedByUserId { get; private set; }

    public DateTime? SuspendedAtUtc { get; private set; }

    public string? SuspensionReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Job(
        Guid id,
        Guid companyId,
        string title,
        bool isForDisabledCandidates,
        Guid employmentTypeId,
        Guid workLocationTypeId,
        Guid positionId,
        Guid departmentId,
        Guid provinceId,
        string? descriptionHtml,
        Guid experienceLevelId,
        DateTime createdAtUtc)
        : base(id)
    {
        CompanyId = companyId;
        Title = title;
        IsForDisabledCandidates = isForDisabledCandidates;
        EmploymentTypeId = employmentTypeId;
        WorkLocationTypeId = workLocationTypeId;
        PositionId = positionId;
        DepartmentId = departmentId;
        ProvinceId = provinceId;
        DescriptionHtml = descriptionHtml;
        ExperienceLevelId = experienceLevelId;
        Status = JobStatus.Draft;
        CreatedAtUtc = createdAtUtc;
    }

    public static Job Create(
        Guid companyId,
        string title,
        bool isForDisabledCandidates,
        Guid employmentTypeId,
        Guid workLocationTypeId,
        Guid positionId,
        Guid departmentId,
        Guid provinceId,
        string? descriptionHtml,
        Guid experienceLevelId,
        IReadOnlyCollection<Guid> genderPreferenceIds,
        IReadOnlyCollection<Guid> militaryStatusPreferenceIds,
        IReadOnlyCollection<Guid> educationLevelPreferenceIds,
        IReadOnlyCollection<Guid> drivingLicensePreferenceIds,
        IReadOnlyCollection<(Guid LanguageId, Guid LanguageLevelId)> languageRequirements,
        DateTime createdAtUtc)
    {
        var job = new Job(
            Guid.NewGuid(), companyId, title, isForDisabledCandidates, employmentTypeId, workLocationTypeId,
            positionId, departmentId, provinceId, descriptionHtml, experienceLevelId, createdAtUtc);

        job.SetGenderPreferences(genderPreferenceIds);
        job.SetMilitaryStatusPreferences(militaryStatusPreferenceIds);
        job.SetEducationLevelPreferences(educationLevelPreferenceIds);
        job.SetDrivingLicensePreferences(drivingLicensePreferenceIds);
        job.SetLanguageRequirements(languageRequirements);

        return job;
    }

    // Yalnızca Draft/RevisionRequested iken düzenlenebilir (AGENTS.md §10: business rule Domain'de
    // yaşar, yalnızca handler'da değil) - Company.Approve/Reject/Deactivate'in Result-döner felsefesiyle
    // tutarlı.
    public Result Update(
        string title,
        bool isForDisabledCandidates,
        Guid employmentTypeId,
        Guid workLocationTypeId,
        Guid positionId,
        Guid departmentId,
        Guid provinceId,
        string? descriptionHtml,
        Guid experienceLevelId,
        IReadOnlyCollection<Guid> genderPreferenceIds,
        IReadOnlyCollection<Guid> militaryStatusPreferenceIds,
        IReadOnlyCollection<Guid> educationLevelPreferenceIds,
        IReadOnlyCollection<Guid> drivingLicensePreferenceIds,
        IReadOnlyCollection<(Guid LanguageId, Guid LanguageLevelId)> languageRequirements)
    {
        if (Status is not (JobStatus.Draft or JobStatus.RevisionRequested))
        {
            return Result.Failure(Error.Conflict("Job.InvalidTransition", $"Cannot update a job while status is {Status}."));
        }

        Title = title;
        IsForDisabledCandidates = isForDisabledCandidates;
        EmploymentTypeId = employmentTypeId;
        WorkLocationTypeId = workLocationTypeId;
        PositionId = positionId;
        DepartmentId = departmentId;
        ProvinceId = provinceId;
        DescriptionHtml = descriptionHtml;
        ExperienceLevelId = experienceLevelId;

        SetGenderPreferences(genderPreferenceIds);
        SetMilitaryStatusPreferences(militaryStatusPreferenceIds);
        SetEducationLevelPreferences(educationLevelPreferenceIds);
        SetDrivingLicensePreferences(drivingLicensePreferenceIds);
        SetLanguageRequirements(languageRequirements);

        return Result.Success();
    }

    // Kullanıcıyla netleştirildi (Görev 2): gerçek akışta ayrı bir "Submitted" ara durumu yok -
    // Draft/RevisionRequested'tan doğrudan UnderReview'a geçer (bkz. JobStatus.cs notu).
    public Result Submit()
    {
        if (Status is not (JobStatus.Draft or JobStatus.RevisionRequested))
        {
            return Result.Failure(Error.Conflict("Job.InvalidTransition", $"Cannot submit a job while status is {Status}."));
        }

        Status = JobStatus.UnderReview;

        return Result.Success();
    }

    // ADR-023 §2: onay ve yayına alma tek adımda birleşiyor, ayrı bir Approved ara durumu yok -
    // doğrudan Published'a geçer.
    public Result Approve(Guid reviewedByAdvisorId, DateTime approvedAtUtc)
    {
        if (Status != JobStatus.UnderReview)
        {
            return Result.Failure(Error.Conflict("Job.InvalidTransition", $"Cannot approve a job while status is {Status}."));
        }

        Status = JobStatus.Published;
        ReviewedByAdvisorId = reviewedByAdvisorId;
        ReviewedAtUtc = approvedAtUtc;
        PublishedAtUtc = approvedAtUtc;

        return Result.Success();
    }

    public Result Reject(string reason, DateTime rejectedAtUtc)
    {
        if (Status != JobStatus.UnderReview)
        {
            return Result.Failure(Error.Conflict("Job.InvalidTransition", $"Cannot reject a job while status is {Status}."));
        }

        Status = JobStatus.Rejected;
        RejectionReason = reason;
        ReviewedAtUtc = rejectedAtUtc;

        return Result.Success();
    }

    public Result RequestRevision(string notes, DateTime requestedAtUtc)
    {
        if (Status != JobStatus.UnderReview)
        {
            return Result.Failure(Error.Conflict("Job.InvalidTransition", $"Cannot request revision while status is {Status}."));
        }

        Status = JobStatus.RevisionRequested;
        RevisionNotes = notes;
        ReviewedAtUtc = requestedAtUtc;

        return Result.Success();
    }

    // PROJECT.md §5.2: admin, yayındaki uygunsuz bir ilanı yayından kaldırabilmeli. Yalnızca
    // Published'tan (CareerAdvisor.Reject/Approve akışının dışında, doğrudan admin aksiyonu).
    public Result Suspend(Guid suspendedByUserId, string reason, DateTime suspendedAtUtc)
    {
        if (Status != JobStatus.Published)
        {
            return Result.Failure(Error.Conflict("Job.InvalidTransition", $"Cannot suspend a job while status is {Status}."));
        }

        Status = JobStatus.SuspendedByAdmin;
        SuspendedByUserId = suspendedByUserId;
        SuspendedAtUtc = suspendedAtUtc;
        SuspensionReason = reason;

        return Result.Success();
    }

    // SuspendedByUserId/SuspendedAtUtc/SuspensionReason bilinçli olarak temizlenmez - geçmiş kaydı
    // olarak kalır, yalnızca Status değişir.
    public Result Reinstate(DateTime reinstatedAtUtc)
    {
        if (Status != JobStatus.SuspendedByAdmin)
        {
            return Result.Failure(Error.Conflict("Job.InvalidTransition", $"Cannot reinstate a job while status is {Status}."));
        }

        Status = JobStatus.Published;

        return Result.Success();
    }

    // "Value-by-set" değiştirme: istemci taraflı bunlar adreslenebilir tekil öğeler değil, düz bir
    // çoklu-seçim listesi (client kendi child-entity id'sini bilmiyor, yalnızca lookup id'lerini
    // gönderiyor) - bu yüzden SocialMediaLink'in add/remove-by-child-id deseni yerine tam değiştirme.
    public void SetGenderPreferences(IReadOnlyCollection<Guid> genderIds)
    {
        _genderPreferences.Clear();
        foreach (var genderId in genderIds.Distinct())
        {
            _genderPreferences.Add(JobGenderPreference.Create(Id, genderId));
        }
    }

    public void SetMilitaryStatusPreferences(IReadOnlyCollection<Guid> militaryStatusIds)
    {
        _militaryStatusPreferences.Clear();
        foreach (var militaryStatusId in militaryStatusIds.Distinct())
        {
            _militaryStatusPreferences.Add(JobMilitaryStatusPreference.Create(Id, militaryStatusId));
        }
    }

    public void SetEducationLevelPreferences(IReadOnlyCollection<Guid> educationLevelIds)
    {
        _educationLevelPreferences.Clear();
        foreach (var educationLevelId in educationLevelIds.Distinct())
        {
            _educationLevelPreferences.Add(JobEducationLevelPreference.Create(Id, educationLevelId));
        }
    }

    public void SetDrivingLicensePreferences(IReadOnlyCollection<Guid> driversLicenseTypeIds)
    {
        _drivingLicensePreferences.Clear();
        foreach (var driversLicenseTypeId in driversLicenseTypeIds.Distinct())
        {
            _drivingLicensePreferences.Add(JobDrivingLicensePreference.Create(Id, driversLicenseTypeId));
        }
    }

    public void SetLanguageRequirements(IReadOnlyCollection<(Guid LanguageId, Guid LanguageLevelId)> languageRequirements)
    {
        _languageRequirements.Clear();
        foreach (var (languageId, languageLevelId) in languageRequirements.Distinct())
        {
            _languageRequirements.Add(JobLanguageRequirement.Create(Id, languageId, languageLevelId));
        }
    }
}
