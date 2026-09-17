using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
public sealed class ExperienceLevel : LookupItem, ILookupItemFactory<ExperienceLevel>
{
    private ExperienceLevel(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private ExperienceLevel()
    {
    }

    public static ReferenceDataLookupType LookupType => ReferenceDataLookupType.ExperienceLevel;

    public static ExperienceLevel Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
