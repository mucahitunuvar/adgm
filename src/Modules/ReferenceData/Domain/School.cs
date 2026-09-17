using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: seeded starting data, Admin CRUD available (ADR-016 Decision 1).
// A named school (e.g. "İstanbul Üniversitesi"), distinct from SchoolCategory (the
// İlkokul/Ortaokul/Lise/Üniversite level). Candidate's education entries treat this as an
// optional reference: if a candidate's school is not yet in this list, they may enter the name
// as free text instead of being forced to pick one (Candidate.md / the Candidate module design ADR).
public sealed class School : LookupItem, ILookupItemFactory<School>
{
    private School(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private School()
    {
    }

    public static ReferenceDataLookupType LookupType => ReferenceDataLookupType.School;

    public static School Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
