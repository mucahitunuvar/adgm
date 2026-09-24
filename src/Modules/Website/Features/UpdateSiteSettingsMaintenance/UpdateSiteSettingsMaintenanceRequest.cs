namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsMaintenance;

public sealed record UpdateSiteSettingsMaintenanceRequest(
    byte[] RowVersion, bool MaintenanceModeEnabled, IReadOnlyList<UpdateSiteSettingsMaintenanceTranslationInput> Translations);
