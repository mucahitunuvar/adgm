using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// SchoolId references ReferenceData's School lookup, but is optional: when the candidate's school
// isn't listed there yet, SchoolNameFreeText carries the name instead (Candidate.md: "Veri
// tabanında varsa seçilecek ... yoksa kullanıcı dileğini yazabilir"). EducationLevelId/
// DiplomaGradingSystemId/ProvinceId reference other ReferenceData lookups, validated at write time
// by the Application layer (ADR-016), not here.
public sealed class Education : Entity
{
    public Guid CandidateCvContentId { get; private set; }

    public Guid EducationLevelId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public EducationCompletionStatus CompletionStatus { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public Guid? DiplomaGradingSystemId { get; private set; }

    public decimal? DiplomaGrade { get; private set; }

    public Guid? SchoolId { get; private set; }

    public string? SchoolNameFreeText { get; private set; }

    public Guid? ProvinceId { get; private set; }

    public string? Description { get; private set; }

    private Education(Guid id, Guid candidateCvContentId, Guid educationLevelId, DateOnly startDate)
        : base(id)
    {
        CandidateCvContentId = candidateCvContentId;
        EducationLevelId = educationLevelId;
        StartDate = startDate;
        CompletionStatus = EducationCompletionStatus.Continuing;
    }

    internal static Education Create(Guid candidateCvContentId, Guid educationLevelId, DateOnly startDate) =>
        new(Guid.NewGuid(), candidateCvContentId, educationLevelId, startDate);

    public void Update(
        Guid educationLevelId,
        DateOnly startDate,
        EducationCompletionStatus completionStatus,
        DateOnly? endDate,
        Guid? diplomaGradingSystemId,
        decimal? diplomaGrade,
        Guid? schoolId,
        string? schoolNameFreeText,
        Guid? provinceId,
        string? description)
    {
        EducationLevelId = educationLevelId;
        StartDate = startDate;
        CompletionStatus = completionStatus;

        // "Terk" seçilirse Bitiş Tarihi/Diploma Not Sistemi/Diploma Notu pasife alınır (Candidate.md).
        var isDropped = completionStatus == EducationCompletionStatus.Dropped;
        EndDate = isDropped ? null : endDate;
        DiplomaGradingSystemId = isDropped ? null : diplomaGradingSystemId;
        DiplomaGrade = isDropped ? null : diplomaGrade;

        SchoolId = schoolId;
        SchoolNameFreeText = schoolId is null ? schoolNameFreeText : null;
        ProvinceId = provinceId;
        Description = description;
    }
}
