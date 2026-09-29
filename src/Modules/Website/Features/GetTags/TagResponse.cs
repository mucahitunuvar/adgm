namespace GenclikMerkezi.Modules.Website.Features.GetTags;

public sealed record TagResponse(Guid Id, string LanguageCode, string Name, string Slug, int UsageCount, DateTime CreatedAtUtc);
