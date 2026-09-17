using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
public sealed class MilitaryStatus : LookupItem, ILookupItemFactory<MilitaryStatus>
{
    private MilitaryStatus(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private MilitaryStatus()
    {
    }

    public static ReferenceDataLookupType LookupType => ReferenceDataLookupType.MilitaryStatus;

    public static MilitaryStatus Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
