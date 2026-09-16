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

    Task<IReadOnlyList<LookupItemSummary>> ListAsync(
        ReferenceDataLookupType type, bool activeOnly = true, CancellationToken cancellationToken = default);

    // For lookups scoped to a parent (District -> Province, TaxOffice -> Province).
    Task<IReadOnlyList<LookupItemSummary>> ListByParentAsync(
        ReferenceDataLookupType type,
        Guid parentId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);
}
