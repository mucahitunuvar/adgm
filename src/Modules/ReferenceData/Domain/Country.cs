namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// SEED: ships via migration only, no admin CRUD endpoint (ADR-016 Decision 1).
public sealed class Country : LookupItem
{
    private Country(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private Country()
    {
    }

    public static Country Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
