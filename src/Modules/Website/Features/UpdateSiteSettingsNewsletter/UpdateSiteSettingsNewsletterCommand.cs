using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsNewsletter;

public sealed record UpdateSiteSettingsNewsletterCommand(byte[] RowVersion, string? NewsletterPrivacyNoticeKey) : IRequest<Result>;
