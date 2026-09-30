namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.5 (Faz 1b Görev 6): GetContentTrash's projection shape - only the requested language's
// title, plus the deletion timestamp and the date it becomes eligible for permanent deletion.
public sealed record ContentItemTrashListItem(
    Guid Id, Guid ContentTypeId, string Title, DateTime DeletedAtUtc, DateTime EligibleForPermanentDeletionAtUtc, byte[] RowVersion);
