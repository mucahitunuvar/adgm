using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;

// Lives outside the Infrastructure namespace (not ReferenceData.Infrastructure) on purpose: both
// ReferenceDataLookupReader (Infrastructure) and the admin-managed CRUD handlers (Features) need
// the exact same key format, and Features must not depend on the Infrastructure namespace
// (ApplicationIndependenceTests) even though ICacheService itself is a plain SharedKernel
// abstraction.
//
// Every key for a given type is built under the same InvalidationPrefix(type) root (ADR-017
// Decision 3), so one ICacheService.RemoveByPrefix call clears every page/filter combination
// ever cached for that type - List and ListByParent included.
public static class ReferenceDataCacheKeys
{
    private static string Prefix(ReferenceDataLookupType type) => $"referencedata:list:{type}:";

    public static string List(ReferenceDataLookupType type, bool activeOnly, PagedRequest paging) =>
        $"{Prefix(type)}{activeOnly}:{paging.Page}:{paging.PageSize}";

    public static string ListByParent(ReferenceDataLookupType type, Guid parentId, bool activeOnly, PagedRequest paging) =>
        $"{Prefix(type)}parent:{parentId}:{activeOnly}:{paging.Page}:{paging.PageSize}";

    public static string InvalidationPrefix(ReferenceDataLookupType type) => Prefix(type);
}
