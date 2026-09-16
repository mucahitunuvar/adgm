namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// SEED: ships via migration only, no admin CRUD endpoint (ADR-016 Decision 1). Code is the BCP-47/
// ISO 639-1 code (e.g. "tr", "en").
public sealed class Language : LookupItem
{
    private Language(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private Language()
    {
    }

    public static Language Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
