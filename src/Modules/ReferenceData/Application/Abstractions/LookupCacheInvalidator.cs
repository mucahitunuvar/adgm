using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Abstractions;

namespace GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;

// Called by the admin-managed CRUD handlers (ADR-016 Decision 3) after every mutation, so
// ReferenceDataLookupReader's cached ListAsync/ListByParentAsync results never go stale for longer
// than it takes the next write to complete. ICacheService is a plain SharedKernel abstraction, not
// module Infrastructure.
public static class LookupCacheInvalidator
{
    public static void Invalidate(ICacheService cacheService, ReferenceDataLookupType type) =>
        cacheService.RemoveByPrefix(ReferenceDataCacheKeys.InvalidationPrefix(type));
}
