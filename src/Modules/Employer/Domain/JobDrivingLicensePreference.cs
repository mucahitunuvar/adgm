using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employer.Domain;

// DriversLicenseTypeId, ReferenceData'nın DriversLicenseType lookup'ına referans verir, write-time'da
// doğrulanmaz (ADR-016) - CandidateLanguage.cs ile aynı desen.
public sealed class JobDrivingLicensePreference : Entity
{
    public Guid JobId { get; private set; }

    public Guid DriversLicenseTypeId { get; private set; }

    private JobDrivingLicensePreference(Guid id, Guid jobId, Guid driversLicenseTypeId)
        : base(id)
    {
        JobId = jobId;
        DriversLicenseTypeId = driversLicenseTypeId;
    }

    internal static JobDrivingLicensePreference Create(Guid jobId, Guid driversLicenseTypeId) =>
        new(Guid.NewGuid(), jobId, driversLicenseTypeId);
}
