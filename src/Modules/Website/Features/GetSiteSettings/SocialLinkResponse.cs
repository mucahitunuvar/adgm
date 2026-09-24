namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

public sealed record SocialLinkResponse(Guid Id, string Platform, string Url, int SortOrder);
