using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact;

public sealed record UpdateSiteSettingsContactCommand(
    byte[] RowVersion,
    string? Address,
    string? Phone,
    string? Email,
    string? WhatsApp,
    string? MapEmbedUrl,
    IReadOnlyList<UpdateSiteSettingsSocialLinkInput> SocialLinks) : IRequest<Result>;
