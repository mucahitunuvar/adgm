using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsEvents;

public sealed record UpdateSiteSettingsEventsCommand(byte[] RowVersion, string? EventPrivacyNoticeKey) : IRequest<Result>;
