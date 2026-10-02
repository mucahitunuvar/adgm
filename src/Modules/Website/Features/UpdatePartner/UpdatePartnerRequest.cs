namespace GenclikMerkezi.Modules.Website.Features.UpdatePartner;

public sealed record UpdatePartnerRequest(byte[] RowVersion, Guid LogoMediaId, string? WebsiteUrl, int SortOrder);
