using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsCookieConsent;

public sealed record UpdateSiteSettingsCookieConsentCommand(
    byte[] RowVersion,
    string? CookiePolicyKey,
    IReadOnlyList<UpdateSiteSettingsCookieConsentTranslationInput> Translations) : IRequest<Result>;
