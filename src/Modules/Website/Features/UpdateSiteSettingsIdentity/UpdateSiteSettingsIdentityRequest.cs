namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;

public sealed record UpdateSiteSettingsIdentityRequest(
    byte[] RowVersion,
    Guid? LogoLightMediaAssetId,
    Guid? LogoDarkMediaAssetId,
    Guid? FaviconMediaAssetId,
    Guid? DefaultOgImageMediaId,
    IReadOnlyList<UpdateSiteSettingsIdentityTranslationInput> Translations);
