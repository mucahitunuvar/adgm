namespace GenclikMerkezi.Modules.Website.Features.RestoreContentItemRevision;

public sealed record RestoreContentItemRevisionRequest(byte[]? RowVersion, string? LanguageCode, bool RestoreCategories);
