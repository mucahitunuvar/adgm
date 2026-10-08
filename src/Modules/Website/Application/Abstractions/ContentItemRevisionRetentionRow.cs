namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public sealed record ContentItemRevisionRetentionRow(Guid Id, int RevisionNumber, bool IsPublishedSnapshot);
