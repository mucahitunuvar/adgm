namespace GenclikMerkezi.Modules.Website.Features.CreatePartner;

public sealed record CreatePartnerRequest(
    Guid LogoMediaId, string? WebsiteUrl, int SortOrder, string? DefaultLanguageName, string? DefaultLanguageDescription);
