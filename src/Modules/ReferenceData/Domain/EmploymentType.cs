using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
// Tam Zamanlı / Yarı Zamanlı.
public sealed class EmploymentType : LookupItem, ILookupItemFactory<EmploymentType>
{
    private EmploymentType(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private EmploymentType()
    {
    }

    public static ReferenceDataLookupType LookupType => ReferenceDataLookupType.EmploymentType;

    public static EmploymentType Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
