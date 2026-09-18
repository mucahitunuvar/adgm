using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Contracts.ReferenceData;

// The published, in-process read contract other modules depend on instead of ReferenceData's own
// DbContext (ADR-016 Decision 2). Implemented in ReferenceData.Infrastructure, registered once at
// the Host composition root, and injected directly - a plain in-process method call, not a network
// request. If ReferenceData is ever extracted to its own deployable, only the implementation behind
// this interface changes.
public interface IReferenceDataLookupReader
{
    Task<bool> ExistsAndActiveAsync(
        ReferenceDataLookupType type, Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<LookupItemSummary>> ListAsync(
        ReferenceDataLookupType type,
        PagedRequest paging,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    // For lookups scoped to a parent (District -> Province, TaxOffice -> Province).
    Task<PagedResult<LookupItemSummary>> ListByParentAsync(
        ReferenceDataLookupType type,
        Guid parentId,
        PagedRequest paging,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    // Batch display-name resolution for a known set of ids (e.g. a listing page's distinct
    // ProvinceId/DistrictId values) - the alternative, calling ListAsync with a large-enough
    // PageSize, breaks down for lookups with hundreds of rows (District) since PageSize is capped at
    // PagedRequest.MaxPageSize. Unlike ListAsync/ListByParentAsync, this never filters by IsActive:
    // the ids passed in already exist on some other record (e.g. a candidate's previously selected
    // province), so a later admin deactivation should not make its display name disappear. An id
    // that doesn't exist at all is simply absent from the result rather than causing a failure.
    Task<IReadOnlyCollection<LookupItemSummary>> GetByIdsAsync(
        ReferenceDataLookupType type, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}
