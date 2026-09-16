namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1). Engelli
// kategorisi.
public sealed class DisabilityCategory : LookupItem
{
    private DisabilityCategory(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private DisabilityCategory()
    {
    }

    public static DisabilityCategory Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
