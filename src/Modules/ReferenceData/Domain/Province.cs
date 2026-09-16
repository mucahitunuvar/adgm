namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// SEED: ships via migration only, no admin CRUD endpoint (ADR-016 Decision 1). "İl" in
// Candidate.md/Employer.md - the same concept as what those forms call "Şehir".
public sealed class Province : LookupItem
{
    public Guid CountryId { get; private set; }

    private Province(Guid id, string code, string displayName, int sortOrder, Guid countryId)
        : base(id, code, displayName, sortOrder)
    {
        CountryId = countryId;
    }

    private Province()
    {
    }

    public static Province Create(string code, string displayName, int sortOrder, Guid countryId) =>
        new(Guid.NewGuid(), code, displayName, sortOrder, countryId);
}
