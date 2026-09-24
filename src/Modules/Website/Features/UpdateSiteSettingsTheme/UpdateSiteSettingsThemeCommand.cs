using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsTheme;

public sealed record UpdateSiteSettingsThemeCommand(
    byte[] RowVersion, string? PrimaryColorHex, string? SecondaryColorHex, string? FontFamily) : IRequest<Result>;
