namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
public sealed class Department : LookupItem
{
    private Department(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private Department()
    {
    }

    public static Department Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
