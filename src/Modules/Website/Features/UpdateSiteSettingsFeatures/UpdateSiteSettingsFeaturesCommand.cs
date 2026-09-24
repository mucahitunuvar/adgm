using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;

public sealed record UpdateSiteSettingsFeaturesCommand(
    byte[] RowVersion,
    bool GlobalSearchEnabled,
    bool NewsletterEnabled,
    bool PublicJobListingsEnabled,
    bool DonationPageEnabled) : IRequest<Result>;
