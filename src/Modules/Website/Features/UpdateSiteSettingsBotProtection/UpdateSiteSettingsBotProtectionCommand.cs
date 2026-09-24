using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBotProtection;

public sealed record UpdateSiteSettingsBotProtectionCommand(byte[] RowVersion, bool BotProtectionEnabled, string? TurnstileSiteKey) : IRequest<Result>;
