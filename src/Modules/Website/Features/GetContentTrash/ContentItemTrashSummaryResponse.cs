namespace GenclikMerkezi.Modules.Website.Features.GetContentTrash;

public sealed record ContentItemTrashSummaryResponse(
    Guid Id, Guid ContentTypeId, string Title, DateTime DeletedAtUtc, DateTime EligibleForPermanentDeletionAtUtc, byte[] RowVersion);
