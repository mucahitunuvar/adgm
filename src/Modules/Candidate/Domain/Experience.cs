using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// CompanyName is free text (Candidate.md: "Firmanın 3 kelimesi girildiğinde bizim sistemde kayıtlı
// ise çıksın" - an autocomplete-over-free-text UX, not a lookup FK; matching against a real Employer
// record is deferred until the Employer module exists, per the Candidate module design ADR).
// PositionId/SectorId/WorkFieldId/EmploymentTypeId/CountryId/ProvinceId reference ReferenceData
// lookups, validated at write time by the Application layer (ADR-016), not here.
public sealed class Experience : Entity
{
    public Guid CandidateCvContentId { get; private set; }

    public string CompanyName { get; private set; }

    public Guid? PositionId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public bool IsCurrentJob { get; private set; }

    public Guid? SectorId { get; private set; }

    public Guid? WorkFieldId { get; private set; }

    public Guid? EmploymentTypeId { get; private set; }

    public Guid? CountryId { get; private set; }

    public Guid? ProvinceId { get; private set; }

    public string? JobDescription { get; private set; }

    private Experience(Guid id, Guid candidateCvContentId, string companyName, DateOnly startDate)
        : base(id)
    {
        CandidateCvContentId = candidateCvContentId;
        CompanyName = companyName;
        StartDate = startDate;
    }

    internal static Experience Create(Guid candidateCvContentId, string companyName, DateOnly startDate) =>
        new(Guid.NewGuid(), candidateCvContentId, companyName, startDate);

    public void Update(
        string companyName,
        Guid? positionId,
        DateOnly startDate,
        DateOnly? endDate,
        bool isCurrentJob,
        Guid? sectorId,
        Guid? workFieldId,
        Guid? employmentTypeId,
        Guid? countryId,
        Guid? provinceId,
        string? jobDescription)
    {
        CompanyName = companyName;
        PositionId = positionId;
        StartDate = startDate;
        // "Halen Çalışıyorum" seçilirse Bitiş Tarihi pasife alınır (Candidate.md).
        EndDate = isCurrentJob ? null : endDate;
        IsCurrentJob = isCurrentJob;
        SectorId = sectorId;
        WorkFieldId = workFieldId;
        EmploymentTypeId = employmentTypeId;
        CountryId = countryId;
        ProvinceId = provinceId;
        JobDescription = jobDescription;
    }
}
