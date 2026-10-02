namespace GenclikMerkezi.Modules.Website.Application.Partners;

public sealed record PublicPartnerResponse(
    Guid Id,
    string Name,
    string? Description,
    string? WebsiteUrl,
    PublicPartnerLogoResponse Logo,
    int SortOrder);
