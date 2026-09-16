namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1). Vergi dairesi,
// bir Province'e bağlı alt lookup (Employer.md: "Vergi Dairesi İli" + "Vergi Dairesi").
public sealed class TaxOffice : LookupItem
{
    public Guid ProvinceId { get; private set; }

    private TaxOffice(Guid id, string code, string displayName, int sortOrder, Guid provinceId)
        : base(id, code, displayName, sortOrder)
    {
        ProvinceId = provinceId;
    }

    private TaxOffice()
    {
    }

    public static TaxOffice Create(string code, string displayName, int sortOrder, Guid provinceId) =>
        new(Guid.NewGuid(), code, displayName, sortOrder, provinceId);
}
