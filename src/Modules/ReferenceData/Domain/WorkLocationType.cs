namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
// Uzaktan / Hibrit / Ofis.
public sealed class WorkLocationType : LookupItem
{
    private WorkLocationType(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private WorkLocationType()
    {
    }

    public static WorkLocationType Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
