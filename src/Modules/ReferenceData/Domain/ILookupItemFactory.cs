using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// Lets the generic admin CRUD handlers (ADR-016 Decision 3) construct any concrete lookup type
// and know its ReferenceDataLookupType without reflection - a static abstract interface member
// (C# 11+), implemented implicitly by each plain admin-managed lookup's existing Create factory.
// TaxOffice does not implement this: it needs a ProvinceId the generic Create command has no slot
// for, so it gets its own small bespoke feature slice instead (ADR-016's own "graduates out of the
// generic pattern" escape hatch).
public interface ILookupItemFactory<TLookup>
    where TLookup : LookupItem
{
    static abstract ReferenceDataLookupType LookupType { get; }

    static abstract TLookup Create(string code, string displayName, int sortOrder);
}
