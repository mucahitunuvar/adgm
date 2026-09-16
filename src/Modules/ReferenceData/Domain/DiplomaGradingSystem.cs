namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1). E.g. 4'lük /
// 100'lük sistem.
public sealed class DiplomaGradingSystem : LookupItem
{
    private DiplomaGradingSystem(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private DiplomaGradingSystem()
    {
    }

    public static DiplomaGradingSystem Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
