using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
// TL / USD / EUR. Code is the ISO 4217 code.
public sealed class Currency : LookupItem, ILookupItemFactory<Currency>
{
    private Currency(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private Currency()
    {
    }

    public static ReferenceDataLookupType LookupType => ReferenceDataLookupType.Currency;

    public static Currency Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
