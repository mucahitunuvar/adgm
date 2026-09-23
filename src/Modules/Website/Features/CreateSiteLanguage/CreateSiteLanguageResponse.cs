namespace GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;

public sealed record CreateSiteLanguageResponse(Guid Id, string Code, string Name, int SortOrder, bool IsDefault, bool IsActive);
