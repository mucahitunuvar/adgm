namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
// İlkokul / Ortaokul / Lise / Üniversite.
public sealed class SchoolCategory : LookupItem
{
    private SchoolCategory(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private SchoolCategory()
    {
    }

    public static SchoolCategory Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
