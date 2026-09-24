using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;

public sealed record UpdateSiteSettingsIdentityCommand(
    byte[] RowVersion,
    Guid? LogoLightMediaAssetId,
    Guid? LogoDarkMediaAssetId,
    Guid? FaviconMediaAssetId,
    Guid? DefaultOgImageMediaId,
    IReadOnlyList<UpdateSiteSettingsIdentityTranslationInput> Translations) : IRequest<Result>;
