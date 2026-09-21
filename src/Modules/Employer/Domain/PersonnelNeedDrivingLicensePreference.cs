using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employer.Domain;

// JobDrivingLicensePreference ile aynı desen: DriversLicenseTypeId ReferenceData'nın
// DriversLicenseType lookup'ına referans verir, write-time'da doğrulanmaz (ADR-016).
public sealed class PersonnelNeedDrivingLicensePreference : Entity
{
    public Guid PersonnelNeedId { get; private set; }

    public Guid DriversLicenseTypeId { get; private set; }

    private PersonnelNeedDrivingLicensePreference(Guid id, Guid personnelNeedId, Guid driversLicenseTypeId)
        : base(id)
    {
        PersonnelNeedId = personnelNeedId;
        DriversLicenseTypeId = driversLicenseTypeId;
    }

    internal static PersonnelNeedDrivingLicensePreference Create(Guid personnelNeedId, Guid driversLicenseTypeId) =>
        new(Guid.NewGuid(), personnelNeedId, driversLicenseTypeId);
}
