using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsMaintenance;

public sealed record UpdateSiteSettingsMaintenanceCommand(
    byte[] RowVersion, bool MaintenanceModeEnabled, IReadOnlyList<UpdateSiteSettingsMaintenanceTranslationInput> Translations) : IRequest<Result>;
