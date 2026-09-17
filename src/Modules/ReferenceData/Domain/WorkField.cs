using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
// Distinct from Sector (industry, e.g. "Bilişim") and Department (org unit, e.g. "İnsan
// Kaynakları"): the broader field of work an experience entry falls under (e.g. "Yazılım
// Geliştirme", "Pazarlama").
public sealed class WorkField : LookupItem, ILookupItemFactory<WorkField>
{
    private WorkField(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private WorkField()
    {
    }

    public static ReferenceDataLookupType LookupType => ReferenceDataLookupType.WorkField;

    public static WorkField Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
