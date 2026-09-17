using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
public sealed class EducationLevel : LookupItem, ILookupItemFactory<EducationLevel>
{
    private EducationLevel(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private EducationLevel()
    {
    }

    public static ReferenceDataLookupType LookupType => ReferenceDataLookupType.EducationLevel;

    public static EducationLevel Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
