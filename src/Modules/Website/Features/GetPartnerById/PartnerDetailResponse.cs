namespace GenclikMerkezi.Modules.Website.Features.GetPartnerById;

public sealed record PartnerDetailResponse(
    Guid Id,
    Guid LogoMediaId,
    PartnerLogoResponse? Logo,
    string? WebsiteUrl,
    int SortOrder,
    bool IsActive,
    byte[] RowVersion,
    IReadOnlyList<PartnerTranslationResponse> Translations,
    DateTime CreatedAtUtc);
