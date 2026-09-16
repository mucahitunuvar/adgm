namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1). Referans tipi
// (Candidate.md "Referans Bilgileri" - e.g. Yönetici, İş Arkadaşı, Akademisyen).
public sealed class ReferenceType : LookupItem
{
    private ReferenceType(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private ReferenceType()
    {
    }

    public static ReferenceType Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
