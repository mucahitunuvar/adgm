using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// Shared shape for every lookup type (ADR-016 Decision 3) - each concrete type still gets its own
// EF-mapped table, never a shared table with a discriminator, since not every lookup has this
// shape alone (District/TaxOffice also carry a ProvinceId). Deactivate is the only removal path:
// other modules may already reference a lookup's id, so a real delete could silently orphan them.
public abstract class LookupItem : Entity
{
    public string Code { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public int SortOrder { get; private set; }

    protected LookupItem(Guid id, string code, string displayName, int sortOrder)
        : base(id)
    {
        Code = code;
        DisplayName = displayName;
        SortOrder = sortOrder;
        IsActive = true;
    }

    protected LookupItem()
    {
    }

    public void Update(string displayName, int sortOrder)
    {
        DisplayName = displayName;
        SortOrder = sortOrder;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
