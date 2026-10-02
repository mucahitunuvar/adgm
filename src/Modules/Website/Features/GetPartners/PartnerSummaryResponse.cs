namespace GenclikMerkezi.Modules.Website.Features.GetPartners;

public sealed record PartnerSummaryResponse(
    Guid Id,
    PartnerLogoResponse? Logo,
    string? WebsiteUrl,
    string Name,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion);
