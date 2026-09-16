namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
public sealed class Gender : LookupItem
{
    private Gender(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private Gender()
    {
    }

    public static Gender Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
