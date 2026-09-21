using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Employer.Domain;

// Aggregate root (ADR-023 §3 / ADR-022 §5). Job'un aksine hiçbir zaman yayınlanmaz - firmanın kendi
// danışmanına düşer, danışman kendi adayları arasında karşılık bulamazsa Genel Havuz'a atar
// (PoolToGeneral). Alan/koleksiyon deseni Job ile birebir aynı (4 child preference collection,
// value-by-set), yalnızca LanguageRequirements yok (ADR-023 §3'te tanımlı değil).
public sealed class PersonnelNeed : AggregateRoot
{
    private readonly List<PersonnelNeedGenderPreference> _genderPreferences = [];
    private readonly List<PersonnelNeedMilitaryStatusPreference> _militaryStatusPreferences = [];
    private readonly List<PersonnelNeedEducationLevelPreference> _educationLevelPreferences = [];
    private readonly List<PersonnelNeedDrivingLicensePreference> _drivingLicensePreferences = [];

    public Guid CompanyId { get; private set; }

    public Guid EmploymentTypeId { get; private set; }

    public Guid WorkLocationTypeId { get; private set; }

    public Guid PositionId { get; private set; }

    public Guid DepartmentId { get; private set; }

    public int Quantity { get; private set; }

    public Guid ProvinceId { get; private set; }

    public Guid ExperienceLevelId { get; private set; }

    public string? DetailsText { get; private set; }

    public IReadOnlyCollection<PersonnelNeedGenderPreference> GenderPreferences => _genderPreferences.AsReadOnly();

    public IReadOnlyCollection<PersonnelNeedMilitaryStatusPreference> MilitaryStatusPreferences => _militaryStatusPreferences.AsReadOnly();

    public IReadOnlyCollection<PersonnelNeedEducationLevelPreference> EducationLevelPreferences => _educationLevelPreferences.AsReadOnly();

    public IReadOnlyCollection<PersonnelNeedDrivingLicensePreference> DrivingLicensePreferences => _drivingLicensePreferences.AsReadOnly();

    public PersonnelNeedStatus Status { get; private set; }

    public Guid? PooledByAdvisorId { get; private set; }

    public DateTime? PooledAtUtc { get; private set; }

    public Guid? ClosedByAdvisorId { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    public Guid? FulfilledByCandidateCvId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private PersonnelNeed(
        Guid id,
        Guid companyId,
        Guid employmentTypeId,
        Guid workLocationTypeId,
        Guid positionId,
        Guid departmentId,
        int quantity,
        Guid provinceId,
        Guid experienceLevelId,
        string? detailsText,
        DateTime createdAtUtc)
        : base(id)
    {
        CompanyId = companyId;
        EmploymentTypeId = employmentTypeId;
        WorkLocationTypeId = workLocationTypeId;
        PositionId = positionId;
        DepartmentId = departmentId;
        Quantity = quantity;
        ProvinceId = provinceId;
        ExperienceLevelId = experienceLevelId;
        DetailsText = detailsText;
        Status = PersonnelNeedStatus.Taslak;
        CreatedAtUtc = createdAtUtc;
    }

    public static PersonnelNeed Create(
        Guid companyId,
        Guid employmentTypeId,
        Guid workLocationTypeId,
        Guid positionId,
        Guid departmentId,
        int quantity,
        Guid provinceId,
        Guid experienceLevelId,
        string? detailsText,
        IReadOnlyCollection<Guid> genderPreferenceIds,
        IReadOnlyCollection<Guid> militaryStatusPreferenceIds,
        IReadOnlyCollection<Guid> educationLevelPreferenceIds,
        IReadOnlyCollection<Guid> drivingLicensePreferenceIds,
        DateTime createdAtUtc)
    {
        var personnelNeed = new PersonnelNeed(
            Guid.NewGuid(), companyId, employmentTypeId, workLocationTypeId, positionId, departmentId,
            quantity, provinceId, experienceLevelId, detailsText, createdAtUtc);

        personnelNeed.SetGenderPreferences(genderPreferenceIds);
        personnelNeed.SetMilitaryStatusPreferences(militaryStatusPreferenceIds);
        personnelNeed.SetEducationLevelPreferences(educationLevelPreferenceIds);
        personnelNeed.SetDrivingLicensePreferences(drivingLicensePreferenceIds);

        return personnelNeed;
    }

    // Job.Update'in aksine (Draft/RevisionRequested) yalnızca Taslak iken düzenlenebilir - master
    // prompt: gönderildikten sonra düzeltme akışı bu görevin kapsamında tanımlı değil.
    public Result Update(
        Guid employmentTypeId,
        Guid workLocationTypeId,
        Guid positionId,
        Guid departmentId,
        int quantity,
        Guid provinceId,
        Guid experienceLevelId,
        string? detailsText,
        IReadOnlyCollection<Guid> genderPreferenceIds,
        IReadOnlyCollection<Guid> militaryStatusPreferenceIds,
        IReadOnlyCollection<Guid> educationLevelPreferenceIds,
        IReadOnlyCollection<Guid> drivingLicensePreferenceIds)
    {
        if (Status != PersonnelNeedStatus.Taslak)
        {
            return Result.Failure(Error.Conflict(
                "PersonnelNeed.InvalidTransition", $"Cannot update a personnel need while status is {Status}."));
        }

        EmploymentTypeId = employmentTypeId;
        WorkLocationTypeId = workLocationTypeId;
        PositionId = positionId;
        DepartmentId = departmentId;
        Quantity = quantity;
        ProvinceId = provinceId;
        ExperienceLevelId = experienceLevelId;
        DetailsText = detailsText;

        SetGenderPreferences(genderPreferenceIds);
        SetMilitaryStatusPreferences(militaryStatusPreferenceIds);
        SetEducationLevelPreferences(educationLevelPreferenceIds);
        SetDrivingLicensePreferences(drivingLicensePreferenceIds);

        return Result.Success();
    }

    public Result Submit()
    {
        if (Status != PersonnelNeedStatus.Taslak)
        {
            return Result.Failure(Error.Conflict(
                "PersonnelNeed.InvalidTransition", $"Cannot submit a personnel need while status is {Status}."));
        }

        Status = PersonnelNeedStatus.KendiHavuzunda;

        return Result.Success();
    }

    public Result PoolToGeneral(Guid pooledByAdvisorId, DateTime pooledAtUtc)
    {
        if (Status != PersonnelNeedStatus.KendiHavuzunda)
        {
            return Result.Failure(Error.Conflict(
                "PersonnelNeed.InvalidTransition", $"Cannot pool a personnel need while status is {Status}."));
        }

        Status = PersonnelNeedStatus.GenelHavuzda;
        PooledByAdvisorId = pooledByAdvisorId;
        PooledAtUtc = pooledAtUtc;

        return Result.Success();
    }

    // KendiHavuzunda VEYA GenelHavuzda'dan çağrılabilir (tasarım kararı - bkz. plan): kendi danışmanı
    // havuza atmadan da kendi adayıyla karşılayabilir, bunu tek yola zorlamak keyfi bir kısıtlama olur.
    public Result Close(Guid closedByAdvisorId, Guid? fulfilledByCandidateCvId, DateTime closedAtUtc)
    {
        if (Status is not (PersonnelNeedStatus.KendiHavuzunda or PersonnelNeedStatus.GenelHavuzda))
        {
            return Result.Failure(Error.Conflict(
                "PersonnelNeed.InvalidTransition", $"Cannot close a personnel need while status is {Status}."));
        }

        Status = PersonnelNeedStatus.Karsilandi;
        ClosedByAdvisorId = closedByAdvisorId;
        FulfilledByCandidateCvId = fulfilledByCandidateCvId;
        ClosedAtUtc = closedAtUtc;

        return Result.Success();
    }

    // "Value-by-set" değiştirme - Job.SetGenderPreferences ile aynı gerekçe: istemci taraflı bunlar
    // adreslenebilir tekil öğeler değil, düz bir çoklu-seçim listesi.
    public void SetGenderPreferences(IReadOnlyCollection<Guid> genderIds)
    {
        _genderPreferences.Clear();
        foreach (var genderId in genderIds.Distinct())
        {
            _genderPreferences.Add(PersonnelNeedGenderPreference.Create(Id, genderId));
        }
    }

    public void SetMilitaryStatusPreferences(IReadOnlyCollection<Guid> militaryStatusIds)
    {
        _militaryStatusPreferences.Clear();
        foreach (var militaryStatusId in militaryStatusIds.Distinct())
        {
            _militaryStatusPreferences.Add(PersonnelNeedMilitaryStatusPreference.Create(Id, militaryStatusId));
        }
    }

    public void SetEducationLevelPreferences(IReadOnlyCollection<Guid> educationLevelIds)
    {
        _educationLevelPreferences.Clear();
        foreach (var educationLevelId in educationLevelIds.Distinct())
        {
            _educationLevelPreferences.Add(PersonnelNeedEducationLevelPreference.Create(Id, educationLevelId));
        }
    }

    public void SetDrivingLicensePreferences(IReadOnlyCollection<Guid> driversLicenseTypeIds)
    {
        _drivingLicensePreferences.Clear();
        foreach (var driversLicenseTypeId in driversLicenseTypeIds.Distinct())
        {
            _drivingLicensePreferences.Add(PersonnelNeedDrivingLicensePreference.Create(Id, driversLicenseTypeId));
        }
    }
}
