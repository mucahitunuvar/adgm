namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1). Ehliyet sınıfı.
public sealed class DriversLicenseType : LookupItem
{
    private DriversLicenseType(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private DriversLicenseType()
    {
    }

    public static DriversLicenseType Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
