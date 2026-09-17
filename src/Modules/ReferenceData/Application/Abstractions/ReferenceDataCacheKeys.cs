using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;

// Lives outside the Infrastructure namespace (not ReferenceData.Infrastructure) on purpose: both
// ReferenceDataLookupReader (Infrastructure) and the admin-managed CRUD handlers (Features) need
// the exact same key format, and Features must not depend on the Infrastructure namespace
// (ApplicationIndependenceTests) even though IMemoryCache itself is a plain framework abstraction.
public static class ReferenceDataCacheKeys
{
    public static string List(ReferenceDataLookupType type, bool activeOnly) =>
        $"referencedata:list:{type}:{activeOnly}";
}
