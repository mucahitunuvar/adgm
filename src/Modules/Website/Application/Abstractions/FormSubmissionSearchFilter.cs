namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §12.2 (Faz 3 Görev 5): the admin list's filters - GetFormSubmissionsQueryHandler builds this
// from the query's own fields and hands it to the repository, the same split GetContentItemsQuery's
// several scalar parameters follow (kept a dedicated type here rather than more scalar repository
// parameters, since this filter has one more field than that call already prefers to inline).
public sealed record FormSubmissionSearchFilter(
    string? FormKey,
    string? Status,
    bool Archived,
    Guid? AssignedToUserId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    string? ReferenceNumber);
