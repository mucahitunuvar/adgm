using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure;

// Shared so the admin-managed CRUD handlers (ADR-016 Decision 3) can invalidate exactly the keys
// ReferenceDataLookupReader populates, without duplicating the key format.
internal static class ReferenceDataCacheKeys
{
    public static string List(ReferenceDataLookupType type, bool activeOnly) =>
        $"referencedata:list:{type}:{activeOnly}";
}
