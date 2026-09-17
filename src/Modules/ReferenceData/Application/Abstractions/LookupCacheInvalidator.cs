using GenclikMerkezi.Contracts.ReferenceData;
using Microsoft.Extensions.Caching.Memory;

namespace GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;

// Called by the admin-managed CRUD handlers (ADR-016 Decision 3) after every mutation, so
// ReferenceDataLookupReader's cached ListAsync results never go stale for longer than it takes the
// next write to complete. IMemoryCache is a plain framework abstraction, not module Infrastructure.
public static class LookupCacheInvalidator
{
    public static void Invalidate(IMemoryCache cache, ReferenceDataLookupType type)
    {
        cache.Remove(ReferenceDataCacheKeys.List(type, activeOnly: true));
        cache.Remove(ReferenceDataCacheKeys.List(type, activeOnly: false));
    }
}
