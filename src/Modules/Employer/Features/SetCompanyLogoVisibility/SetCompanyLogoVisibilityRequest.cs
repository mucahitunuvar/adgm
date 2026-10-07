namespace GenclikMerkezi.Modules.Employer.Features.SetCompanyLogoVisibility;

public sealed record SetCompanyLogoVisibilityRequest(bool ShowLogoOnWebsite, byte[] RowVersion);
