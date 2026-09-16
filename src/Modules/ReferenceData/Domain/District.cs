namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// SEED: ships via migration only, no admin CRUD endpoint (ADR-016 Decision 1). "İlçe", scoped to
// a Province.
public sealed class District : LookupItem
{
    public Guid ProvinceId { get; private set; }

    private District(Guid id, string code, string displayName, int sortOrder, Guid provinceId)
        : base(id, code, displayName, sortOrder)
    {
        ProvinceId = provinceId;
    }

    private District()
    {
    }

    public static District Create(string code, string displayName, int sortOrder, Guid provinceId) =>
        new(Guid.NewGuid(), code, displayName, sortOrder, provinceId);
}
