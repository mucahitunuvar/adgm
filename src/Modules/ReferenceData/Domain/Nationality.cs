using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1). Distinct from
// Country: a person's Nationality (uyruk) is not the same lookup as where a Province/District
// belongs to (Country), even though many entries will mirror each other in practice.
public sealed class Nationality : LookupItem, ILookupItemFactory<Nationality>
{
    private Nationality(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private Nationality()
    {
    }

    public static ReferenceDataLookupType LookupType => ReferenceDataLookupType.Nationality;

    public static Nationality Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
